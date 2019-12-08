using System;
using System.Collections.Generic;
using System.Text;

namespace otor.msixhero.lib.Domain.Appx.Manifest.Build
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
