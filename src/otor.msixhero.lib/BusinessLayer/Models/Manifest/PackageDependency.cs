using System;

namespace otor.msixhero.lib.BusinessLayer.Models.Manifest
{
    [Serializable]
    public class PackageDependency
    {
    
        public string Name { get; set; }

        public string Version { get; set; }
        
        public string Publisher { get; set; }
    }
}