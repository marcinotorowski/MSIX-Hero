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
using System.Text.RegularExpressions;

namespace Otor.MsixHero.Infrastructure.Configuration.ResolvableFolder
{
    public class ResolvablePath
    {
        private static readonly Lazy<List<FolderDefinition>> PathLookup = new Lazy<List<FolderDefinition>>(GeneratePaths);

        private string _resolved;
        private string _compacted;

        public ResolvablePath()
        {
        }

        public ResolvablePath(string compactedPath)
        {
            this.SetFromCompacted(compactedPath);
        }

        public string Resolved
        {
            get => this._resolved;
            set => this.SetFromResolved(value);
        }

        public string Compacted
        {
            get => this._compacted;
            set => this.SetFromCompacted(value);
        }

        public static implicit operator string(ResolvablePath folder)
        {
            return folder?.Resolved;
        }

        public static implicit operator ResolvablePath(string folderPath)
        {
            return new ResolvablePath { Resolved = string.IsNullOrEmpty(folderPath) ? null : folderPath };
        }

        private static List<FolderDefinition> GeneratePaths()
        {
            var list = new List<FolderDefinition>();

            var values = Enum.GetValues(typeof(Environment.SpecialFolder)).OfType<Environment.SpecialFolder>();
            foreach (var item in values)
            {
                list.Add(new FolderDefinition("{{" + item.ToString("G") + "}}", Environment.GetFolderPath(item, Environment.SpecialFolderOption.DoNotVerify)));
            }

            return list.OrderByDescending(x => x.Path.Split('\\').Length).ThenByDescending(x => x.Path.Length).ToList();
        }

        private void SetFromResolved(string resolvedPath)
        {
            this._resolved = resolvedPath;
            this._compacted = resolvedPath;

            if (string.IsNullOrEmpty(resolvedPath))
            {
                return;
            }

            foreach (var item in PathLookup.Value)
            {
                if (string.IsNullOrEmpty(item.Path))
                {
                    continue;
                }

                var indexOf = resolvedPath.IndexOf(item.Path, StringComparison.OrdinalIgnoreCase);
                if (indexOf == -1)
                {
                    continue;
                }

                resolvedPath = resolvedPath.Remove(indexOf, item.Path.Length).Insert(indexOf, item.Key);
            }

            this._compacted = resolvedPath;
        }

        private void SetFromCompacted(string compactedPath)
        {
            this._compacted = compactedPath;
            this._resolved = compactedPath;

            if (string.IsNullOrEmpty(compactedPath))
            {
                return;
            }

            this._resolved = Regex.Replace(compactedPath, "{{([a-zA-Z0-9]+)}}", match =>
            {
                var matchedValue = match.Groups[1].Value;
                
                // ReSharper disable once ConvertIfStatementToReturnStatement
                if (!Enum.TryParse(matchedValue, true, out Environment.SpecialFolder parsed))
                {
                    return matchedValue;
                }

                return Environment.GetFolderPath(parsed, Environment.SpecialFolderOption.DoNotVerify);
            });
        }

        private struct FolderDefinition
        {
            public FolderDefinition(string key, string path)
            {
                this.Key = key;
                this.Path = path;
            }

            public string Key { get; private set; }

            public string Path { get; private set; }

            public override string ToString()
            {
                return $"{this.Key}: {this.Path}";
            }
        }
    }
}