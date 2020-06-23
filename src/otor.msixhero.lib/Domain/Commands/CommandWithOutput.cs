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
    [XmlInclude(typeof(DismountRegistry))]
    [XmlInclude(typeof(FindUsers))]
    [XmlInclude(typeof(RemovePackages))]
    [XmlInclude(typeof(SetPackageSorting))]
    [XmlInclude(typeof(GetLogs))]
    [XmlInclude(typeof(SetPackageGrouping))]
    [XmlInclude(typeof(GetPackageDetails))]
    [XmlInclude(typeof(InstallCertificate))]
    [XmlInclude(typeof(TrustPublisher))]
    public abstract class VoidCommand
    {
    }

    // ReSharper disable once UnusedTypeParameter
    public abstract class CommandWithOutput<T> : VoidCommand
    {
    }
}