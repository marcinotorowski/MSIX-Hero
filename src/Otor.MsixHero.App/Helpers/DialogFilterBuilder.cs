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
using Otor.MsixHero.Appx.Packaging;

namespace Otor.MsixHero.App.Helpers
{
    /// <summary>
    /// A builder for dialog filters in Windows style.
    /// </summary>
    public class DialogFilterBuilder
    {
        private readonly HashSet<string> filters = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        
        public DialogFilterBuilder(params string[] extensions)
        {
            this.AddFilters(extensions);
        }
        
        public void AddFilters(params string[] extensions)
        {
            foreach (var ext in extensions)
            {
                // ReSharper disable once StringLiteralTypo
                if (string.Equals(ext, FileConstants.AppxManifestFile, StringComparison.OrdinalIgnoreCase))
                {
                    // ReSharper disable once StringLiteralTypo
                    this.filters.Add(FileConstants.AppxManifestFile);
                    continue;
                }
                
                this.filters.Add(ext);
            }
        }
        
        public string BuildFilter(bool includeAllSupported = true, bool includeAll = true)
        {
            var filter = string.Join('|', new[]
            {
                this.BuildPackagesFilter(),
                this.BuildBundles(),
                this.BuildManifestFilter(),
                this.BuildWinget(),
                this.BuildAppInstaller(),
                this.BuildCertificateFiles(),
                this.BuildRegistry(),
                this.BuildLegacyInstallers(),
                this.BuildOtherFiles()
            }.Where(s => !string.IsNullOrEmpty(s)));
            
            if (!includeAll && !includeAllSupported)
            {
                return filter;
            }    
            
            var sections = filter.Split('|');
            if (sections.Length == 0)
            {
                return Resources.Localization.Dialogs_Filter_AllFiles + "|*.*";
            }

            var allExtensions = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            for (var i = 1; i < sections.Length; i += 2)
            {
                var extensions = sections[i].Split(';');
                foreach (var extension in extensions)
                {
                    allExtensions.Add(extension);
                }
            }
            
            if (includeAllSupported && allExtensions.Count > 1 && sections.Length / 2 > 1)
            {
                // if there is just one category then it makes no sense to show all supported files extra
                filter = Resources.Localization.Dialogs_Filter_AllSupportedFiles + "|" + string.Join(';', allExtensions.OrderBy(e => e)) + "|" + filter;
            }

            if (includeAll && !allExtensions.Contains("*.*"))
            {
                if (filter.Length > 0)
                {
                    filter += "|" + Resources.Localization.Dialogs_Filter_AllFiles + "|*.*";
                }
                else
                {
                    filter = Resources.Localization.Dialogs_Filter_AllFiles + "|*.*";
                }
            }

            return filter;
        }

        private string BuildLegacyInstallers()
        {
            var msi = this.filters.Contains("*.msi");
            var exe = this.filters.Contains("*.exe");
            
            if (msi)
            {
                if (exe)
                {
                    return Resources.Localization.Dialogs_Filter_Msi + "|*.msi|" + Resources.Localization.Dialogs_Filter_Exe + "|*.exe";
                }

                return Resources.Localization.Dialogs_Filter_Msi + "|*.msi";
            }

            if (exe)
            {
                return Resources.Localization.Dialogs_Filter_Exe + "|*.exe";
            }

            return null;
        }
        
        private string BuildOtherFiles()
        {
            var otherExtensions = this.filters.Select(filter => 
            {
                switch (filter.ToLowerInvariant())
                {
                    // ReSharper disable once StringLiteralTypo
                    case FileConstants.AppxManifestFile:
                    case "*" + FileConstants.MsixExtension:
                    case "*" + FileConstants.AppxExtension:
                    case "*" + FileConstants.WingetExtension:
                    // ReSharper disable once StringLiteralTypo
                    case "*" + FileConstants.AppInstallerExtension:
                    case "*.reg":
                    case "*.cer":
                    case "*.exe":
                    case "*.msi":
                    case "*.pfx":
                    // ReSharper disable once StringLiteralTypo
                    case "*" + FileConstants.AppxBundleExtension:
                    // ReSharper disable once StringLiteralTypo
                    case "*" + FileConstants.MsixBundleExtension:
                        return null;
                    default:
                        return filter;
                }
            }).Where(ext => ext != null).OrderBy(ext => ext);
            
            return string.Join('|', otherExtensions.Select(e => string.Format(Resources.Localization.Dialogs_Filter_MultipleFormats, e.TrimStart('*').TrimStart('.').ToUpperInvariant() + "|*." + e)));
        }

        private string BuildPackagesFilter()
        {
            var msix = this.filters.Contains("*" + FileConstants.MsixExtension);
            var appx = this.filters.Contains("*" + FileConstants.AppxExtension);
            
            var package = msix || appx;
            if (!package)
            {
                return string.Empty;
            }

            var stringBuilder = new StringBuilder();
            stringBuilder.Append(Resources.Localization.Dialogs_Filter_Packages + "|");
            if (msix)
            {
                stringBuilder.Append("*" + FileConstants.MsixExtension + ";");
            }

            if (appx)
            {
                stringBuilder.Append("*" + FileConstants.MsixExtension + ";");
            }

            return stringBuilder.ToString().TrimEnd(';');
        }

        private string BuildManifestFilter()
        {
            // ReSharper disable once StringLiteralTypo
            return this.filters.Contains(FileConstants.AppxManifestFile) ? Resources.Localization.Dialogs_Filter_Manifests + "|" + FileConstants.AppxManifestFile : null;
        }

        private string BuildBundles()
        {
            var msixBundle = this.filters.Contains("*" + FileConstants.MsixBundleExtension);
            var appxBundle = this.filters.Contains("*" + FileConstants.AppxBundleExtension);
            
            var bundle = msixBundle || appxBundle;
            if (!bundle)
            {
                return string.Empty;
            }

            var stringBuilder = new StringBuilder();
            stringBuilder.Append(Resources.Localization.Dialogs_Filter_Bundles + "|");
            
            if (msixBundle)
            {
                stringBuilder.Append("*" + FileConstants.MsixBundleExtension + ";");
            }

            if (appxBundle)
            {
                stringBuilder.Append("*" + FileConstants.AppxBundleExtension + ";");
            }

            return stringBuilder.ToString().TrimEnd(';');
        }

        private string BuildWinget()
        {
            // ReSharper disable once StringLiteralTypo
            return this.filters.Contains("*" + FileConstants.WingetExtension) ? Resources.Localization.Dialogs_Filter_Winget + "|*" + FileConstants.WingetExtension : null;
        }

        private string BuildCertificateFiles()
        {
            var pfx = this.filters.Contains("*.pfx");
            var cer = this.filters.Contains("*.cer");

            if (!pfx && !cer)
            {
                return null;
            }

            var stringBuilder = new StringBuilder(Resources.Localization.Dialogs_Filter_Certificates + "|");

            if (pfx)
            {
                stringBuilder.Append("*.pfx;");
            }

            if (cer)
            {
                stringBuilder.Append("*.cer;");
            }

            return stringBuilder.ToString().TrimEnd(';');
        }

        private string BuildRegistry()
        {
            return this.filters.Contains("*.reg") ? Resources.Localization.Dialogs_Filter_Registry + "|*.reg" : null;
        }
        
        private string BuildAppInstaller()
        {
            // ReSharper disable once StringLiteralTypo
            return this.filters.Contains("*" + FileConstants.AppInstallerExtension) ? Resources.Localization.Dialogs_Filter_AppInstaller + "|*" + FileConstants.AppInstallerExtension : null;
        }
    }
}