using System;

namespace otor.msixhero.lib.BusinessLayer.Models.Manifest.Full
{
    [Serializable]
    public class AppxTargetOperatingSystem
    {
        public string Name { get; set; }

        public string TechnicalVersion { get; set; }
        
        public string MarketingCodename { get; set; }
    }
}
