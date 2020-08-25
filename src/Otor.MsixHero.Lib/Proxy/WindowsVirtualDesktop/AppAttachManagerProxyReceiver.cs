using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Otor.MsixHero.Appx.WindowsVirtualDesktop.AppAttach;
using Otor.MsixHero.Infrastructure.Processes.Ipc;
using Otor.MsixHero.Infrastructure.Processes.SelfElevation.Enums;
using Otor.MsixHero.Infrastructure.Progress;
using Otor.MsixHero.Lib.Proxy.WindowsVirtualDesktop.Dto;

namespace Otor.MsixHero.Lib.Proxy.WindowsVirtualDesktop
{
    public class AppAttachManagerProxyReceiver : SelfElevationProxyReceiver<IAppAttachManager>
    {
        public AppAttachManagerProxyReceiver(IAppAttachManager selfElevationAware) : base(selfElevationAware)
        {
        }

        public override IEnumerable<Type> GetSupportedProxiedObjectTypes()
        {
            yield return typeof(CreateVolumeDto);
        }

        public override Task<TCommandResult> Get<TCommandResult>(IProxyObjectWithOutput<TCommandResult> command, CancellationToken cancellationToken = default, IProgress<ProgressData> progress = null)
        {
            throw new InvalidOperationException("This command does not return anything.");
        }

        public override Task Invoke(IProxyObject command, CancellationToken cancellationToken = default, IProgress<ProgressData> progress = null)
        {
            var proxiedObject = (CreateVolumeDto)command;
            return this.SelfElevationAwareObject.CreateVolume(proxiedObject.PackagePath, proxiedObject.VhdPath, proxiedObject.SizeInMegaBytes, proxiedObject.ExtractCertificate, proxiedObject.GenerateScripts, cancellationToken, progress);
        }
    }
}
