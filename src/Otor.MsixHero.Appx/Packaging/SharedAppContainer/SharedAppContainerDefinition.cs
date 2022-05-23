using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Otor.MsixHero.Appx.Packaging.SharedAppContainer
{
    public class SharedAppContainerDefinition
    {
        /*
         *
<?xml version="1.0" encoding="utf-8"?> 
<AppSharedPackageContainer Name="ContosoContainer"> 
  <PackageFamily Name="Fabrikam.MainApp_8wekyb3d8bbwe"/> 
  <PackageFamily Name="Contoso.MainApp_8wekyb3d8bbwe"/> 
  <PackageFamily Name="ContosoCustomize_7xekyb3d8ccde"/> 
</AppSharedPackageContainer>   
         */

        [XmlAttribute("Name")]
        public string Name { get; set; }

        [XmlElement("PackageFamily")]
        public List<PackageFamilyDefinition> PackageFamilies { get; set; }
    }
}
