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
using System.Diagnostics;
using System.IO;
using Otor.MsixHero.Appx.Diagnostic.Developer.Enums;
using Dapplo.Log;

namespace Otor.MsixHero.Appx.Diagnostic.Developer
{
    public class SideloadingConfigurator : BaseWindowsSideloadingConfigurator
    {
        private static readonly LogSource Logger = new();
        private ISideloadingConfigurator sideloadingConfigurator;

        public override SideloadingFlavor Flavor
        {
            get
            {
                this.AssertConfiguratorSetForWindowsVersion();
                return this.sideloadingConfigurator.Flavor;
            }
        }

        public override bool Set(SideloadingStatus status)
        {
            this.AssertConfiguratorSetForWindowsVersion();
            return this.sideloadingConfigurator.Set(status);
        }

        public override SideloadingStatus Get()
        {
            this.AssertConfiguratorSetForWindowsVersion();
            return this.sideloadingConfigurator.Get();
        }

        private void AssertConfiguratorSetForWindowsVersion()
        {
            if (this.sideloadingConfigurator != null)
            {
                return;
            }

            string kernel32Version;

            try
            {
                // This trick to check Windows version uses the fact that kernel32.dll seems to be reasonably versioned.
                // Builds 10.22000 and higher are Windows 11.
                kernel32Version = FileVersionInfo.GetVersionInfo(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System), "kernel32.dll")).ProductVersion;
            }
            catch (Exception e)
            {
                throw new ApplicationException("Could not determine the version of your Windows.", e);
            }

            if (!Version.TryParse(kernel32Version, out var version))
            {
                throw new ApplicationException($"Could not determine the version of your Windows. Version of kernel32.dll is {kernel32Version}");
            }

            if (version.Major != 10)
            {
                throw new ApplicationException("MSIX Hero only supports Windows 10 and Windows 11. Your major version is " + version.Major);
            }

            if (version.Build >= 22000)
            {
                // we are on Windows 11
                Logger.Info().WriteLine("Initializing sideloading checker for Windows 11…");
                this.sideloadingConfigurator = new Windows11SideloadingConfigurator();
            }
            else if (version.Build >= 19041)
            {
                // we are on Windows 10 2004 (May 2002 Update) or newer
                Logger.Info().WriteLine("Initializing sideloading checker for Windows 10 2004 (May Update) or newer…");
                this.sideloadingConfigurator = new Windows10From2004SideloadingConfigurator();
            }
            else
            {
                // we are on Windows 10
                Logger.Info().WriteLine("Initializing sideloading checker for Windows 10 versions below 2004 (May Update)…");
                this.sideloadingConfigurator = new Windows10Below2004SideloadingConfigurator();
            }
        }
    }
}