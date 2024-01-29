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
using System.Reflection;
using System.Threading.Tasks;
using System.Xml.Linq;
using Otor.MsixHero.Appx.Editor.Commands.Concrete.Manifest;
using Otor.MsixHero.Appx.Editor.Executors.Concrete.Manifest;
using Otor.MsixHero.Infrastructure.Helpers;
using Otor.MsixHero.Infrastructure.ThirdParty.Sdk;

namespace Otor.MsixHero.Appx.Editor.Facades
{
    public class MsixHeroBrandingInjector
    {
        public enum BrandingInjectorOverrideOption
        {
            Default, // will prefer existing with exception of MsixHero, makeappx.exe and signtool.exe which must be taken over from the current toolset
            PreferExisting, // will prefer existing values and never overwrite anything with exception of MsixHero
            PreferIncoming // will replace existing values with new ones
        }

        public async Task Inject(XDocument manifestContent, BrandingInjectorOverrideOption overwrite = BrandingInjectorOverrideOption.Default)
        {
            var toWrite = new Dictionary<string, string>();
            var toWriteOnlyIfMissing = new Dictionary<string, string>();

            var signToolVersion = GetVersion("signtool.exe");
            var makePriVersion = GetVersion("makepri.exe");
            var makeAppxVersion = GetVersion("makeappx.exe");
            var operatingSystemVersion = NdDll.RtlGetVersion().ToString();
            // ReSharper disable once PossibleNullReferenceException
            var msixHeroVersion = (Assembly.GetEntryAssembly() ?? Assembly.GetExecutingAssembly()).GetName().Version.ToString();

            toWrite.Add("MsixHero", msixHeroVersion);

            switch (overwrite)
            {
                case BrandingInjectorOverrideOption.Default:
                    // prefer existing, with exception of signtool and makeappx that are always re-created.
                    toWriteOnlyIfMissing.Add("MakePri.exe", makePriVersion);
                    toWrite.Add("SignTool.exe", signToolVersion);
                    toWrite.Add("MakeAppx.exe", makeAppxVersion);
                    toWriteOnlyIfMissing.Add("OperatingSystem", operatingSystemVersion);
                    break;
                case BrandingInjectorOverrideOption.PreferExisting:
                    // prefer all existing
                    toWriteOnlyIfMissing.Add("MakePri.exe", makePriVersion);
                    toWriteOnlyIfMissing.Add("SignTool.exe", signToolVersion);
                    toWriteOnlyIfMissing.Add("MakeAppx.exe", makeAppxVersion);
                    toWriteOnlyIfMissing.Add("OperatingSystem", operatingSystemVersion);
                    break;
                case BrandingInjectorOverrideOption.PreferIncoming:
                    // overwrite everything
                    toWrite.Add("MakePri.exe", makePriVersion);
                    toWrite.Add("SignTool.exe", signToolVersion);
                    toWrite.Add("MakeAppx.exe", makeAppxVersion);
                    toWrite.Add("OperatingSystem", operatingSystemVersion);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(overwrite), overwrite, null);
            }

            var executor = new SetBuildMetaDataExecutor(manifestContent);

            if (toWrite.Any())
            {
                var action = new SetBuildMetaData(toWrite)
                {
                    OnlyCreateNew = false
                };
                
                await executor.Execute(action).ConfigureAwait(false);
            }
            
            if (toWriteOnlyIfMissing.Any())
            {
                var action = new SetBuildMetaData(toWriteOnlyIfMissing)
                {
                    OnlyCreateNew = true
                };

                await executor.Execute(action).ConfigureAwait(false);
            }
        }

        private static string GetVersion(string sdkFile)
        {
            var path = SdkPathHelper.GetSdkPath(sdkFile);
            return File.Exists(path) ? FileVersionInfo.GetVersionInfo(path).ProductVersion : null;
        }
    }
}
