using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Otor.MsixHero.Appx.Signing;
using Otor.MsixHero.Infrastructure.Processes.Ipc;
using Otor.MsixHero.Infrastructure.Processes.SelfElevation.Enums;
using Otor.MsixHero.Infrastructure.Progress;
using Otor.MsixHero.Lib.Proxy.Signing.Dto;

namespace Otor.MsixHero.Lib.Proxy.Signing
{
    public class SigningManagerProxyReceiver : SelfElevationProxyReceiver<ISigningManager>
    {
        public SigningManagerProxyReceiver(ISigningManager selfElevationAware) : base(selfElevationAware)
        {
        }

        public override IEnumerable<Type> GetSupportedProxiedObjectTypes()
        {
            yield return typeof(InstallCertificateDto);
            yield return typeof(TrustDto);
        }

        public override Task<TCommandResult> Get<TCommandResult>(IProxyObjectWithOutput<TCommandResult> command, CancellationToken cancellationToken = default, IProgress<ProgressData> progress = null)
        {
            throw new NotSupportedException();
        }

        public override Task Invoke(IProxyObject command, CancellationToken cancellationToken = default, IProgress<ProgressData> progress = null)
        {
            if (command is InstallCertificateDto installCertificateDto)
            {
                return this.SelfElevationAwareObject.InstallCertificate(installCertificateDto.FilePath, cancellationToken, progress);
            }

            if (command is TrustDto trustDto)
            {
                return this.SelfElevationAwareObject.Trust(trustDto.FilePath, cancellationToken);
            }

            throw new NotSupportedException();
        }
    }
}
