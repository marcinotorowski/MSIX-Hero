using System;

namespace otor.msixhero.lib.Domain.Appx.Manifest.Full
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