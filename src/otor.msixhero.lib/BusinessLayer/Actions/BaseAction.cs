using System;
using System.Xml.Serialization;

namespace otor.msixhero.lib.BusinessLayer.Actions
{
    [Serializable]
    [XmlInclude(typeof(ReloadPackages))]
    [XmlInclude(typeof(SelectPackages))]
    [XmlInclude(typeof(BaseElevatedAction))]
    [XmlInclude(typeof(SetPackageFilter))]
    [XmlInclude(typeof(SetPackageContext))]
    [XmlInclude(typeof(SetPackageSidebarVisibility))]
    public abstract class BaseAction
    {
    }
}