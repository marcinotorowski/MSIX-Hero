using System;
using System.Xml.Serialization;
using Otor.MsixHero.Appx.Packaging.Installation.Entities;

namespace Otor.MsixHero.Lib.Proxy.Packaging.Dto
{
    [Serializable]
    public class RunToolInContextDto : ProxyObject
    {
        public RunToolInContextDto(string packagePackageFamilyName, string packageName, string toolPath, string arguments = null)
        {
            this.PackageFamilyName = packagePackageFamilyName;
            this.AppId = packageName;
            this.ToolPath = toolPath;
            this.Arguments = arguments;
        }

        public RunToolInContextDto(InstalledPackage package, string toolPath, string arguments = null) : this(package.PackageFamilyName, package.Name, toolPath, arguments)
        {
        }

        public RunToolInContextDto()
        {
        }

        [XmlElement]
        public string PackageFamilyName { get; set; }

        [XmlElement]
        public string AppId { get; set; }
        
        [XmlElement]
        public string ToolPath { get; set; }
        
        [XmlElement]
        public string Arguments { get; set; }
    }
}
