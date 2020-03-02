using System;
using System.Xml.Serialization;
using otor.msixhero.lib.Domain.Commands.Packages.Developer;
using otor.msixhero.lib.Domain.Commands.Packages.Grid;
using otor.msixhero.lib.Domain.Commands.Packages.Manager;
using otor.msixhero.lib.Domain.Commands.Packages.Signing;

namespace otor.msixhero.lib.Domain.Commands
{
    [Serializable]
    [XmlInclude(typeof(GetPackages))]
    [XmlInclude(typeof(SelectPackages))]
    [XmlInclude(typeof(SetPackageFilter))]
    [XmlInclude(typeof(RunPackage))]
    [XmlInclude(typeof(SetPackageSidebarVisibility))]
    [XmlInclude(typeof(MountRegistry))]
    [XmlInclude(typeof(UnmountRegistry))]
    [XmlInclude(typeof(GetUsersOfPackage))]
    [XmlInclude(typeof(FindUsers))]
    [XmlInclude(typeof(RemovePackages))]
    [XmlInclude(typeof(SetPackageSorting))]
    [XmlInclude(typeof(GetLogs))]
    [XmlInclude(typeof(SetPackageGrouping))]
    [XmlInclude(typeof(GetPackageDetails))]
    [XmlInclude(typeof(InstallCertificate))]
    public abstract class BaseCommand
    {
    }

    public abstract class BaseCommand<T> : BaseCommand
    {
    }
}