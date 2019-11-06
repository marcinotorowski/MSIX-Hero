using System;
using System.Xml.Serialization;
using otor.msixhero.lib.BusinessLayer.Commands.Developer;
using otor.msixhero.lib.BusinessLayer.Commands.Grid;
using otor.msixhero.lib.BusinessLayer.Commands.Manager;
using otor.msixhero.lib.BusinessLayer.Commands.Signing;
using otor.msixhero.lib.BusinessLayer.Commands.UI;

namespace otor.msixhero.lib.BusinessLayer.Commands
{
    [Serializable]
    [XmlInclude(typeof(GetPackages))]
    [XmlInclude(typeof(SelectPackages))]
    [XmlInclude(typeof(SetPackageFilter))]
    [XmlInclude(typeof(SetPackageContext))]
    [XmlInclude(typeof(SetPackageSidebarVisibility))]
    [XmlInclude(typeof(MountRegistry))]
    [XmlInclude(typeof(UnmountRegistry))]
    [XmlInclude(typeof(GetUsersOfPackage))]
    [XmlInclude(typeof(FindUsers))]
    [XmlInclude(typeof(RemovePackages))]
    [XmlInclude(typeof(SetPackageSorting))]
    [XmlInclude(typeof(GetLogs))]
    [XmlInclude(typeof(SetPackageGrouping))]
    [XmlInclude(typeof(GetManifestedDetails))]
    [XmlInclude(typeof(InstallCertificate))]
    public abstract class BaseCommand
    {
    }

    public abstract class BaseCommand<T> : BaseCommand
    {
    }
}