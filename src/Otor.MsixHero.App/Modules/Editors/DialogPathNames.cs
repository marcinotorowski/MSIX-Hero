namespace Otor.MsixHero.App.Modules.Editors
{
    public static class DialogPathNames
    {
        public const string AppAttachEditor = DialogModuleNames.AppAttach + "-editor";
        public const string AppInstallerEditor = DialogModuleNames.AppInstaller + "-editor";
        public const string DependenciesGraph = DialogModuleNames.Dependencies + "-graph";

        public const string PackagingPack = DialogModuleNames.Packaging + "-pack";
        public const string PackagingUnpack = DialogModuleNames.Packaging + "-unpack";
        public const string PackagingModificationPackage = DialogModuleNames.Packaging + "-modification-package";

        public const string SigningCertificateExport = DialogModuleNames.Signing + "-certificate-export";
        public const string SigningNewSelfSigned = DialogModuleNames.Signing + "-new-self-signed";
        public const string SigningPackageSigning = DialogModuleNames.Signing + "-package-signing";

        public const string UpdatesUpdateImpact = DialogModuleNames.Updates + "-update-impact";

        public const string VolumesNewVolume = DialogModuleNames.Volumes + "-new-volume";
        public const string VolumesChangeVolume = DialogModuleNames.Volumes + "-change-volume";

        public const string WingetYamlEditor = DialogModuleNames.Winget + "-yaml-editor";

    }
}
