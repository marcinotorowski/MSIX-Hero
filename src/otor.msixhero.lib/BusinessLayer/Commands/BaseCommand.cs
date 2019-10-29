using System;
using System.Xml.Serialization;

namespace otor.msixhero.lib.BusinessLayer.Commands
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
    [XmlInclude((typeof(GetSelectionDetails)))]
    public abstract class BaseCommand
    {
    }
}