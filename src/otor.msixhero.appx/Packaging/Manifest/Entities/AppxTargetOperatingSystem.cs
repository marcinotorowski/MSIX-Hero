using System;
using Otor.MsixHero.Appx.Packaging.Manifest.Enums;

namespace Otor.MsixHero.Appx.Packaging.Manifest.Entities
{
    [Serializable]
    public class AppxTargetOperatingSystem
    {
        public string Name { get; set; }
        
        public string NativeFamilyName { get; set; }

        public string TechnicalVersion { get; set; }
        
        public string MarketingCodename { get; set; }

        public AppxTargetOperatingSystemType IsNativeMsixPlatform { get; set; }

        public override string ToString()
        {
            return string.IsNullOrEmpty(this.MarketingCodename) ? $"{this.Name} ({this.TechnicalVersion})" : $"{this.Name} {this.MarketingCodename} ({this.TechnicalVersion})";
        }
    }
}
