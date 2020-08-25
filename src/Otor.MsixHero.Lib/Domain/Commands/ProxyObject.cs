using System;
using System.Xml.Serialization;
using Otor.MsixHero.Infrastructure.Processes.Ipc;
using Otor.MsixHero.Lib.Proxy.Diagnostic.Dto;
using Otor.MsixHero.Lib.Proxy.Packaging.Dto;
using Otor.MsixHero.Lib.Proxy.Signing.Dto;

namespace Otor.MsixHero.Lib.Domain.Commands
{
    [Serializable]
    [XmlInclude(typeof(GetInstalledPackagesDto))]
    [XmlInclude(typeof(RunDto))]
    [XmlInclude(typeof(MountRegistryDto))]
    [XmlInclude(typeof(DismountRegistryDto))]
    [XmlInclude(typeof(GetUsersForPackageDto))]
    [XmlInclude(typeof(RemoveDto))]
    [XmlInclude(typeof(GetLogsDto))]
    [XmlInclude(typeof(GetByIdentityDto))]
    [XmlInclude(typeof(GetByManifestPathDto))]
    [XmlInclude(typeof(InstallCertificateDto))]
    [XmlInclude(typeof(TrustDto))]
    public abstract class ProxyObject : IProxyObject
    {
    }

    // ReSharper disable once UnusedTypeParameter
    public abstract class ProxyObject<T> : ProxyObject, IProxyObjectWithOutput<T>
    {
    }
}