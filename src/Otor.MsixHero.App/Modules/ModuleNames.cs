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

namespace Otor.MsixHero.App.Modules
{
    public static class ModuleNames
    {
        public const string Main = "msix-hero-main-module";
        public const string PackageManagement = "msix-hero-package-management-module";
        public const string VolumeManagement = "msix-hero-volume-management-module";
        public const string EventViewer = "msix-hero-event-viewer-module";
        public const string Containers = "msix-hero-containers-module";
        public const string SystemStatus = "msix-hero-system-status-module";
        public const string Tools = "msix-hero-tools-tools-module";
        public const string WhatsNew = "msix-hero-tools-whatsnew-module";

        public static class Dialogs
        {
            private const string Base = "msix-hero-dialog-";

            public const string AppAttach = Base + "app-attach";
            public const string AppInstaller = Base + "appinstaller";
            public const string Dependencies = Base + "dependencies";
            public const string Packaging = Base + "app-packaging";
            public const string Signing = Base + "signing";
            public const string Updates = Base + "updates";
            public const string Volumes = Base + "volumes";
            // ReSharper disable once IdentifierTypo
            public const string Winget = Base + "winget";
            public const string Settings = Base + "settings";
            public const string About = Base + "about";
        }
    }
}
