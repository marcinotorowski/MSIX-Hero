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

namespace Otor.MsixHero.App.Modules
{
    public static class NavigationPaths
    {
        private const string Base = "msix-hero-";
        private const string BaseNavigation = Base + "navigation-";

        public const string PackageManagement = BaseNavigation + "package-management";
        public const string VolumeManagement = BaseNavigation + "volume-management";
        public const string EventViewer = BaseNavigation + "event-viewer";
        public const string Containers = BaseNavigation + "containers";
        public const string SystemStatus = BaseNavigation + "system-status";
        public const string Tools = BaseNavigation + "dasboard";
        public const string WhatsNew = BaseNavigation + "whatsnew";
        public const string Empty = BaseNavigation + "empty";

        public static class PackageManagementPaths
        {
            private const string BasePackageManagementNavigation = BaseNavigation + "-package-management";

            public const string Search = BasePackageManagementNavigation + "search";
            public const string SingleSelection = BasePackageManagementNavigation + "single";
            public const string MultipleSelection = BasePackageManagementNavigation + "multiple";
            public const string ZeroSelection = BasePackageManagementNavigation + "nothing";
        }

        public static class VolumeManagementPaths
        {
            private const string BaseVolumeManagementNavigation = BaseNavigation + "volume-management";

            public const string Search = BaseVolumeManagementNavigation + "search";
            public const string SingleSelection = BaseVolumeManagementNavigation + "single";
            public const string MultipleSelection = BaseVolumeManagementNavigation + "multiple";
            public const string ZeroSelection = BaseVolumeManagementNavigation + "nothing";
        }

        public static class EventViewerPaths
        {
            private const string BaseEventViewerNavigation = BaseNavigation + "event-viewer";

            public const string Search = BaseEventViewerNavigation + "search";
            public const string Details = BaseEventViewerNavigation + "details";
            public const string NoDetails = BaseEventViewerNavigation + "no-details";
        }

        public static class ContainersPaths
        {
            private const string BaseContainersNavigation = BaseNavigation + "containers";

            public const string Search = BaseContainersNavigation + "search";
            public const string Details = BaseContainersNavigation + "details";
            public const string NoDetails = BaseContainersNavigation + "no-details";
        }

        public static class ToolsPaths
        {
            private const string BaseDashboardNavigation = BaseNavigation + "dashboard";

            public const string Search = BaseDashboardNavigation + "search";
        }
        
        public static class DialogPaths
        {
            private const string BaseDialogsNavigation = BaseNavigation + "dialogs";
            
            public const string AppAttachEditor = BaseDialogsNavigation + "appattach-editor";
            public const string AppInstallerEditor = BaseDialogsNavigation + "appinstaller-editor";
            public const string DependenciesGraph = BaseDialogsNavigation + "dependencies-graph";
            public const string PackageExpert = BaseDialogsNavigation + "package-expert";

            public const string PackagingPack = BaseDialogsNavigation + "packaging-pack";
            public const string PackagingNames = BaseDialogsNavigation + "packaging-names";
            public const string PackagingUnpack = BaseDialogsNavigation + "packaging-unpack";
            public const string PackagingSharedPackageContainer = BaseDialogsNavigation + "shared-package-container";
            public const string PackagingModificationPackage = BaseDialogsNavigation + "packaging-modification-package";

            public const string SigningCertificateExport = BaseDialogsNavigation + "signing-certificate-export";
            public const string SigningNewSelfSigned = BaseDialogsNavigation + "signing-new-self-signed";
            public const string SigningPackageSigning = BaseDialogsNavigation + "signing-package-signing";

            public const string UpdatesUpdateImpact = BaseDialogsNavigation + "updates-update-impact";

            public const string VolumesNewVolume = BaseDialogsNavigation + "volumes-new-volume";
            public const string VolumesChangeVolume = BaseDialogsNavigation + "volumes-change-volume";

            // ReSharper disable once IdentifierTypo
            public const string WingetYamlEditor = BaseDialogsNavigation + "winget-yaml-editor";

            public const string Settings = BaseDialogsNavigation + "settings";
            
            public const string About = BaseDialogsNavigation + "about";
        }
    }
}
