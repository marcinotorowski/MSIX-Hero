using System;
using System.Xml.Serialization;

namespace otor.msixhero.lib.BusinessLayer.Actions
{
    [Serializable]
    public class UnmountRegistry : BaseElevatedAction
    {
        public UnmountRegistry()
        {
        }

        public UnmountRegistry(string packageName)
        {
            this.PackageName = packageName;
        }

        public UnmountRegistry(Package package)
        {
            this.PackageName = package.Name;
        }

        [XmlElement]
        public string PackageName { get; set; }

        [XmlIgnore]
        public override bool RequiresElevation => true;
    }
}