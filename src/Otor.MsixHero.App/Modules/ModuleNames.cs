namespace Otor.MsixHero.App.Modules
{
    public static class ModuleNames
    {
        public const string Main = "msix-hero-main-module";
        public const string PackageManagement = "msix-hero-package-management-module";
        public const string VolumeManagement = "msix-hero-volume-management-module";
        public const string EventViewer = "msix-hero-event-viewer-module";
        public const string SystemView = "msix-hero-system-view-module";
        public const string Dashboard = "msix-hero-tools-dashboard-module";

        public static class Dialogs
        {
            private const string Base = "msix-hero-dialog-";

            public const string AppAttach = Base + "app-attach";
            public const string AppInstaller = Base + "app-appinstaller";
            public const string Dependencies = Base + "app-dependencies";
            public const string Packaging = Base + "app-packaging";
            public const string Signing = Base + "app-signing";
            public const string Updates = Base + "app-updates";
            public const string Volumes = Base + "app-volumes";
            // ReSharper disable once IdentifierTypo
            public const string Winget = Base + "app-winget";
        }
    }
}
