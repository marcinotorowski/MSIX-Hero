// MSIX Hero
// Copyright (C) 2022 Marcin Otorowski
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// Full notice:
// https://github.com/marcinotorowski/msix-hero/blob/develop/LICENSE.md

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Otor.MsixHero.App.Helpers.Dialogs
{
    /// <summary>
    /// A builder for dialog filters in Windows style.
    /// </summary>
    public class DialogFilterBuilder
    {
        private readonly Dictionary<string, FilterEntry> _filters = new(StringComparer.OrdinalIgnoreCase);
        private bool _includeAllSupported;
        
        public DialogFilterBuilder WithAllSupported()
        {
            this._includeAllSupported = true;
            return this;
        }

        public DialogFilterBuilder WithAll(string displayName = null)
        {
            if (displayName == null)
            {
                displayName = Resources.Localization.AllFiles;
            }

            if (displayName.IndexOf('|') != -1)
            {
                throw new ArgumentException(@"The display name must not contain pipe character (|).", nameof(displayName));
            }

            this._filters["*.*"] = new FilterEntry(displayName, int.MaxValue, FilterEntryType.AllFiles);
            return this;
        }

        public DialogFilterBuilder WithExtension(
            string extension, 
            string displayName = null, 
            int order = 0)
        {
            if (extension == null)
            {
                throw new ArgumentNullException(nameof(extension));
            }

            if (extension == "*" || extension == "*.*" || extension == ".*")
            {
                return this.WithAll(displayName);
            }

            if (extension.IndexOf('|') != -1)
            {
                throw new ArgumentException(@"The extension must not contain pipe character (|).", nameof(extension));
            }

            if (displayName == null)
            {
                displayName = string.Format(Resources.Localization.Dialogs_Filter_MultipleFormats, extension.TrimStart('.').ToUpperInvariant());
            }

            if (displayName.IndexOf('|') != -1)
            {
                throw new ArgumentException(@"The display name must not contain pipe character (|).", nameof(displayName));
            }

            if (extension.StartsWith("*.", StringComparison.OrdinalIgnoreCase))
            {
                // ok
            }
            else if (extension.StartsWith('.'))
            {
                extension = "*" + extension;
            }
            else
            {
                if (extension.IndexOf('.') != -1 || extension.IndexOf('*') != -1)
                {
                    throw new ArgumentException(@"Extension format not supported.", nameof(extension));
                }

                extension = "*." + extension;
            }

            if (displayName.IndexOf('|') != -1)
            {
                displayName = displayName.Replace('|', ' ');
            }

            this._filters[extension] = new FilterEntry(displayName, order, FilterEntryType.Extension);
            return this;
        }

        public DialogFilterBuilder WithFile(
            string fileName, 
            string displayName = null, 
            int order = 0)
        {
            if (fileName == null)
            {
                throw new ArgumentNullException(nameof(fileName));
            }

            if (fileName.IndexOf('|') != -1)
            {
                throw new ArgumentException(@"The file name must not contain pipe character (|).", nameof(fileName));
            }

            if (displayName == null)
            {
                displayName = string.Format(Resources.Localization.Dialogs_Filter_MultipleFormats, fileName);
            }

            if (displayName != null && displayName.IndexOf('|') != -1)
            {
                throw new ArgumentException(@"The display name must not contain pipe character (|).", nameof(displayName));
            }

            if (fileName == "*.*")
            {
                return this.WithAll(displayName);
            }

            this._filters[fileName] = new FilterEntry(displayName, order, FilterEntryType.File);
            
            return this;
        }
        
        public DialogFilterBuilder WithWindowsFilter(
            string mask, 
            int order = 0)
        {
            if (mask == null)
            {
                throw new ArgumentNullException(nameof(mask));
            }

            var split = mask.Split('|');
            for (var i = 1; i < split.Length; i += 2)
            {
                var displayName = split[i - 1];
                var extensions = split[i].Split(';');

                foreach (var ext in extensions)
                {
                    if (ext.StartsWith("*.", StringComparison.OrdinalIgnoreCase))
                    {
                        this.WithExtension(ext, displayName, order);
                    }
                    else
                    {
                        this.WithFile(ext, displayName, order);
                    }
                }
            }

            return this;
        }
        
        public string Build()
        {
            var stringBuilder = new StringBuilder();
            foreach (var group in this._filters.Where(f => f.Value.Type != FilterEntryType.AllFiles).GroupBy(f => f.Value.DisplayName).OrderBy(g => g.Min(x => x.Value.Order)))
            {
                var ext = string.Join(';', group.Select(g => g.Key));

                stringBuilder.Append('|');
                stringBuilder.Append(group.Key);

                if (group.Any(x => x.Value.Type != FilterEntryType.Extension))
                {
                    stringBuilder.Append(" (");
                    stringBuilder.Append(ext);
                    stringBuilder.Append(")");
                }

                stringBuilder.Append('|');
                stringBuilder.Append(ext);
            }
            
            if (this._includeAllSupported)
            {
                stringBuilder.Append('|');
                stringBuilder.Append(Resources.Localization.Dialogs_Filter_AllSupportedFiles);
                stringBuilder.Append('|');
                stringBuilder.Append(string.Join(';', this._filters.Where(f => f.Value.Type != FilterEntryType.AllFiles).Select(g => g.Key).Distinct()));
            }

            if (!this._filters.Any() || this._filters.Any(f => f.Value.Type == FilterEntryType.AllFiles))
            {
                stringBuilder.Append('|');
                stringBuilder.Append(Resources.Localization.AllFiles.Replace('|', ' '));
                stringBuilder.Append('|');
                stringBuilder.Append("*.*");
            }

            return stringBuilder.ToString(1, stringBuilder.Length - 1);
        }

        public override string ToString()
        {
            return this.Build();
        }

        public static implicit operator string(DialogFilterBuilder builder)
        {
            return builder.ToString();
        }

        private struct FilterEntry
        {
            public FilterEntry(string displayName, int order, FilterEntryType type)
            {
                this.DisplayName = displayName;
                this.Order = order;
                this.Type = type;
            }

            public string DisplayName { get; }

            public int Order { get; }

            public FilterEntryType Type { get; }
        }

        private enum FilterEntryType
        {
            File,
            Extension,
            AllFiles
        }
    }
}