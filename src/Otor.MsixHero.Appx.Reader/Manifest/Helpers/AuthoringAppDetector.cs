// MSIX Hero
// Copyright (C) 2024 Marcin Otorowski
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
using System.Diagnostics;
using System.IO;
using System.Linq;
using Otor.MsixHero.Appx.Common.WindowsVersioning;
using Otor.MsixHero.Appx.Reader.File;
using Otor.MsixHero.Appx.Reader.Manifest.Entities.Build;

namespace Otor.MsixHero.Appx.Reader.Manifest.Helpers
{
    public class AuthoringAppDetector
    {
        private readonly IAppxFileReader _fileReader;

        public AuthoringAppDetector(IAppxFileReader fileReader)
        {
            this._fileReader = fileReader;
        }

        public bool TryDetectAny(IReadOnlyDictionary<string, string> buildKeyValues, out BuildInfo buildInfo)
        {
            return this.TryDetectAdvancedInstaller(buildKeyValues, out buildInfo)
                   || this.TryDetectVisualStudio(buildKeyValues, out buildInfo)
                   || this.TryDetectRayPack(buildKeyValues, out buildInfo)
                   || this.TryDetectMsixHero(buildKeyValues, out buildInfo);
        }
        
        public bool TryDetectVisualStudio(IReadOnlyDictionary<string, string> buildValues, out BuildInfo buildInfo)
        {
            buildInfo = null;

            if (buildValues == null || !buildValues.Any())
            {
                return false;
            }

            if (!buildValues.TryGetValue("VisualStudio", out var visualStudio))
            {
                return false;
            }

            buildInfo = new BuildInfo
            {
                ProductName = "Microsoft Visual Studio",
                ProductVersion = visualStudio
            };

            buildInfo.Components = new Dictionary<string, string>(buildValues);

            if (buildValues.TryGetValue("OperatingSystem", out var win10))
            {
                var firstUnit = win10.Split(' ')[0];
                buildInfo.OperatingSystem = WindowsNames.GetOperatingSystemFromNameAndVersion(firstUnit);
            }

            return true;
        }

        public bool TryDetectAdvancedInstaller(IReadOnlyDictionary<string, string> buildValues, out BuildInfo buildInfo)
        {
            buildInfo = null;

            if (buildValues == null || !buildValues.Any())
            {
                return false;
            }

            if (!buildValues.TryGetValue("AdvancedInstaller", out var advInst))
            {
                return false;
            }

            buildValues.TryGetValue("ProjectLicenseType", out var projLic);
            buildInfo = new BuildInfo
            {
                ProductLicense = projLic,
                ProductName = "Advanced Installer",
                ProductVersion = advInst
            };

            buildInfo.Components = new Dictionary<string, string>(buildValues);

            if (buildValues.TryGetValue("OperatingSystem", out var os))
            {
                var win10Version = WindowsNames.GetOperatingSystemFromNameAndVersion(os);
                buildInfo.OperatingSystem = win10Version;
            }

            return true;
        }

        public bool TryDetectMsixHero(IReadOnlyDictionary<string, string> buildValues, out BuildInfo buildInfo)
        {
            buildInfo = null;

            if (buildValues == null || !buildValues.Any())
            {
                return false;
            }

            if (!buildValues.TryGetValue("MsixHero", out var msixHero))
            {
                return false;
            }

            buildInfo = new BuildInfo
            {
                ProductName = "MSIX Hero",
                ProductVersion = msixHero
            };

            buildInfo.Components = new Dictionary<string, string>(buildValues);

            if (buildValues.TryGetValue("OperatingSystem", out var os))
            {
                var win10Version = WindowsNames.GetOperatingSystemFromNameAndVersion(os);
                buildInfo.OperatingSystem = win10Version;
            }

            return true;
        }

        public bool TryDetectRayPack(IReadOnlyDictionary<string, string> buildValues, out BuildInfo buildInfo)
        {
            buildInfo = null;

            if (buildValues == null || !buildValues.Any())
            {
                // Detect RayPack by taking a look at the metadata of PsfLauncher.
                // This is a fallback in case there are no other build values.
                const string fileLauncher = "PsfLauncher.exe";
                if (!_fileReader.FileExists(fileLauncher))
                {
                    return false;
                }

                using var launcher = this._fileReader.GetFile(fileLauncher);
                FileVersionInfo fileVersionInfo;
                if (launcher is FileStream fileStream)
                {
                    fileVersionInfo = FileVersionInfo.GetVersionInfo(fileStream.Name);
                }
                else
                {
                    var tempFilePath = Path.GetTempFileName();
                    try
                    {
                        using (var fs = System.IO.File.OpenWrite(tempFilePath))
                        {
                            launcher.CopyTo(fs);
                            fs.Flush();
                        }

                        fileVersionInfo = FileVersionInfo.GetVersionInfo(tempFilePath);
                    }
                    finally
                    {
                        System.IO.File.Delete(tempFilePath);
                    }
                }

                if (fileVersionInfo.ProductName != null && fileVersionInfo.ProductName.StartsWith("Raynet", StringComparison.OrdinalIgnoreCase))
                {
                    var pv = fileVersionInfo.ProductVersion;
                    buildInfo = new BuildInfo
                    {
                        ProductVersion = fileVersionInfo.ProductVersion
                    };

                    if (pv == null)
                    {
                        buildInfo.ProductName = "RayPack";
                    }
                    else
                    {
                        buildInfo.ProductName = "RayPack " + Version.Parse(pv).ToString(2);
                    }

                    return true;
                }
            }
            else
            {
                // Detect RayPack 6.2 which uses build meta data like this:
                // <build:Item Name="OperatingSystem" Version="6.2.9200.0" /><build:Item Name="Raynet.RaySuite.Common.Appx" Version="6.2.5306.1168" /></build:Metadata>
                if (!buildValues.TryGetValue("Raynet.RaySuite.Common.Appx", out var rayPack))
                {
                    return false;
                }
                
                if (Version.TryParse(rayPack, out var parsedVersion))
                {
                    buildInfo = new BuildInfo
                    {
                        ProductName = $"RayPack {parsedVersion.Major}.{parsedVersion.Minor}",
                        ProductVersion = $"(MSIX builder v{parsedVersion})"
                    };
                }
                else
                {
                    buildInfo = new BuildInfo
                    {
                        ProductName = "RayPack",
                        ProductVersion = $"(MSIX builder v{rayPack})"
                    };
                }
                
                buildInfo.Components = new Dictionary<string, string>(buildValues);
                if (buildValues.TryGetValue("OperatingSystem", out var os))
                {
                    var win10Version = WindowsNames.GetOperatingSystemFromNameAndVersion(os);
                    buildInfo.OperatingSystem = win10Version;
                }
                
                return true;
            }

            return false;
        }
    }
}
