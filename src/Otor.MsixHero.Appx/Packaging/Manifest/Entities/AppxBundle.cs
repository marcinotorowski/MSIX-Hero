using System;
using System.Collections.Generic;

namespace Otor.MsixHero.Appx.Packaging.Manifest.Entities
{
    [Serializable]
    public class AppxBundle
    {
        public string RootFolder { get; set; }

        public string Name { get; set; }

        public string Path { get; set; }

        public string Publisher { get; set; }

        public string Version { get; set; }

        public IList<AppxPackage> Packages { get; set; }
    }
}