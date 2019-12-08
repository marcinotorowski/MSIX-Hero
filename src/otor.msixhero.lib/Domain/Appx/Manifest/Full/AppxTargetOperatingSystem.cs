using System;

namespace otor.msixhero.lib.Domain.Appx.Manifest.Full
{
    [Serializable]
    public class AppxTargetOperatingSystem
    {
        public string Name { get; set; }

        public string TechnicalVersion { get; set; }
        
        public string MarketingCodename { get; set; }

        public override string ToString()
        {
            return string.IsNullOrEmpty(this.MarketingCodename) ? $"{this.Name} ({this.TechnicalVersion})" : $"{this.Name} {this.MarketingCodename} ({this.TechnicalVersion})";
        }
    }
}
