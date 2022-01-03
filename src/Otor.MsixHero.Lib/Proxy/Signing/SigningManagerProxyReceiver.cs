// MSIX Hero
// Copyright (C) 2022 Marcin Otorowski
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// Full notice:
// https://github.com/marcinotorowski/msix-hero/blob/develop/LICENSE.md

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
