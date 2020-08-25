using System.Collections.Generic;

namespace Otor.MsixHero.Appx.Packaging.Manifest.Entities.Build
{
    public class BuildInfo
    {
        public string OperatingSystem { get; set; }

        public string ProductName { get; set; }

        public string ProductVersion { get; set; }

        public string ProductLicense { get; set; }
        
        public Dictionary<string, string> Components { get; set; }
    }
}
