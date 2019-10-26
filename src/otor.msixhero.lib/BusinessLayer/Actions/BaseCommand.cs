using System;
using System.Xml.Serialization;

namespace otor.msixhero.lib.BusinessLayer.Actions
{
    [Serializable]
    [XmlInclude(typeof(GetPackages))]
    [XmlInclude(typeof(SelectPackages))]
    [XmlInclude(typeof(BaseSelfElevatedBaseCommand))]
    [XmlInclude(typeof(SetPackageFilter))]
    [XmlInclude(typeof(SetPackageContext))]
    [XmlInclude(typeof(SetPackageSidebarVisibility))]
    [XmlInclude(typeof(MountRegistry))]
    [XmlInclude(typeof(UnmountRegistry))]
    [XmlInclude((typeof(GetUsersOfPackage)))]
    [XmlInclude((typeof(RemovePackage)))]
    public abstract class BaseCommand
    {
    }
}