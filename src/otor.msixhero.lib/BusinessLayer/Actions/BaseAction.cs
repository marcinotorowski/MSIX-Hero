using System;
using System.Xml.Serialization;

namespace otor.msixhero.lib.BusinessLayer.Actions
{
    [Serializable]
    [XmlInclude(typeof(GetPackages))]
    [XmlInclude(typeof(SelectPackages))]
    [XmlInclude(typeof(BaseElevatedAction))]
    [XmlInclude(typeof(SetPackageFilter))]
    [XmlInclude(typeof(SetPackageContext))]
    [XmlInclude(typeof(SetPackageSidebarVisibility))]
    [XmlInclude(typeof(MountRegistry))]
    [XmlInclude(typeof(UnmountRegistry))]
    [XmlInclude((typeof(FindUsersOfPackage)))]
    public abstract class BaseAction
    {
    }
}