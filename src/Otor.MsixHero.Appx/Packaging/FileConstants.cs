using System.Diagnostics.CodeAnalysis;

namespace Otor.MsixHero.Appx.Packaging
{
    [SuppressMessage("ReSharper", "IdentifierTypo")]
    public static class FileConstants
    {
        public const string MsixExtension = ".msix";
        public const string AppxExtension = ".appx";
        
        public const string WingetExtension = ".yaml";
        public const string MsixBundleExtension = ".msixbundle";
        public const string AppxBundleExtension = ".appxbundle";
        public const string AppxManifestFile = "appxmanifest.xml";
        public const string AppxBundleManifestFile = "appxbundlemanifest.xml";
        public const string AppxBundleManifestFilePath = "appxmetadata/" + AppxBundleManifestFile;
        public const string AppInstallerExtension = ".appinstaller";
        public const string AppAttachVhdExtension = ".vhd";
        public const string AppAttachVhdxExtension = ".vhdx";
        public const string AppAttachCimExtension = ".cim";
        public const string RegExtension = ".reg";
        public const string PfxExtension = ".pfx";
        public const string CerExtension = ".cer";
        public const string ExeExtension = ".exe";
        public const string MsiExtension = ".msi";
    }
}
