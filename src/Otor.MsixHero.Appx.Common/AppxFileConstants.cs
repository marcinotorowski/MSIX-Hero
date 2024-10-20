using System.Diagnostics.CodeAnalysis;

namespace Otor.MsixHero.Appx.Common
{
    [SuppressMessage("ReSharper", "IdentifierTypo")]
    public static class AppxFileConstants
    {
        public const string AppxManifestFile = "appxmanifest.xml";
        public const string AppxBundleManifestFile = "appxbundlemanifest.xml";
        public const string AppxBundleManifestFilePath = "appxmetadata/" + AppxBundleManifestFile;
    }
}
