using System;

namespace Otor.MsixHero.Dependencies.Domain
{
    public class OperatingSystemGraphElement : GraphElement
    {
        public OperatingSystemGraphElement(int id, string operatingSystem) : base(id)
        {
            this.OperatingSystem = operatingSystem;
        }

        public string OperatingSystem { get; }

        public Version MaxRequiredVersion { get; set; }
        public string MaxRequiredCaption { get; set; }

        public override string ToString()
        {
            return this.OperatingSystem;
        }
    }
}