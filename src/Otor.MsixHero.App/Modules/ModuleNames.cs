namespace Otor.MsixHero.App.Modules
{
    public static class ModuleNames
    {
        public const string Main = "msix-hero-main-module";
        public const string PackageManagement = "msix-hero-package-management-module";
        public const string VolumeManagement = "msix-hero-volume-management-module";
        public const string EventViewer = "msix-hero-event-viewer-module";
        public const string SystemStatus = "msix-hero-system-status-module";
        public const string Dashboard = "msix-hero-tools-dashboard-module";
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
            public const string Help = Base + "help";
        }
    }
}
