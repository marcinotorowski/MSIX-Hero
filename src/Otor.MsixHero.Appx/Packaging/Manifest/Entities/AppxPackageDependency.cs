using System;
using Microsoft.PowerShell.Commands;

namespace Otor.MsixHero.Appx.Packaging.Manifest.Entities
{
    [Serializable]
    public class AppxPackageDependency
    {
        public string Name { get; set; }

        public string Version { get; set; }
        
        public string Publisher { get; set; }

        public AppxPackage Dependency { get; set; }
    }
}