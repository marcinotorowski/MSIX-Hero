using System;

namespace Otor.MsixHero.Appx.Packaging.Manifest.Entities.Sources
{
    public class AppInstallerPackageSource : AppxSource
    {
        public AppInstallerPackageSource(Uri appInstallerUri, string rootDirectory) : base(rootDirectory)
        {
            this.AppInstallerUri = appInstallerUri;
        }

        public Uri AppInstallerUri { get; }
    }
}