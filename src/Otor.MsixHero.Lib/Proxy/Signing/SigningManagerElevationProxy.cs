// MSIX Hero
// Copyright (C) 2021 Marcin Otorowski
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
using System.IO;
using System.Security;
using System.Threading;
using System.Threading.Tasks;
using Otor.MsixHero.Appx.Signing;
using Otor.MsixHero.Appx.Signing.DeviceGuard;
using Otor.MsixHero.Appx.Signing.Entities;
using Otor.MsixHero.Infrastructure.Processes.Ipc;
using Otor.MsixHero.Infrastructure.Processes.SelfElevation;
using Otor.MsixHero.Infrastructure.Processes.SelfElevation.Enums;
using Otor.MsixHero.Infrastructure.Progress;
using Otor.MsixHero.Lib.Proxy.Signing.Dto;

namespace Otor.MsixHero.Lib.Proxy.Signing
{
    public class SigningManagerElevationProxy : ISigningManager
    {
        private readonly ISelfElevationProxyProvider<ISigningManager> managerFactory;
        private readonly IElevatedClient client;

        public SigningManagerElevationProxy(IElevatedClient client, ISelfElevationProxyProvider<ISigningManager> managerFactory)
        {
            this.managerFactory = managerFactory;
            this.client = client;
        }

        public Task InstallCertificate(string certificateFilePath, CancellationToken cancellationToken = default, IProgress<ProgressData> progress = default)
        {
            var proxyObject = new InstallCertificateDto(certificateFilePath);
            return client.Invoke(proxyObject, cancellationToken, progress);
        }

        public Task Trust(string certificateOrSignedFilePath, CancellationToken cancellationToken = default)
        {
            var proxyObject = new TrustDto(certificateOrSignedFilePath);
            return client.Invoke(proxyObject, cancellationToken);
        }

        public async Task ImportCertificateFromMsix(string msixFile, CancellationToken cancellationToken = default, IProgress<ProgressData> progress = null)
        {
            var manager = await this.managerFactory.GetProxyFor(SelfElevationLevel.AsInvoker, cancellationToken).ConfigureAwait(false);
            cancellationToken.ThrowIfCancellationRequested();
            await manager.ImportCertificateFromMsix(msixFile, cancellationToken, progress).ConfigureAwait(false);
        }

        public async Task<string> CreateSelfSignedCertificate(
            DirectoryInfo outputDirectory, 
            string publisherName, 
            string publisherDisplayName, 
            string password,
            DateTime? validUntil,
            CancellationToken cancellationToken = default, 
            IProgress<ProgressData> progress = null)
        {
            var manager = await this.managerFactory.GetProxyFor(SelfElevationLevel.AsInvoker, cancellationToken).ConfigureAwait(false);
            cancellationToken.ThrowIfCancellationRequested();
            return await manager.CreateSelfSignedCertificate(outputDirectory, publisherName, publisherDisplayName, password, validUntil, cancellationToken, progress).ConfigureAwait(false);
        }

        public async Task<TrustStatus> IsTrusted(string certificateFileOrSignedFile, CancellationToken cancellationToken = default)
        {
            var manager = await this.managerFactory.GetProxyFor(SelfElevationLevel.AsInvoker, cancellationToken).ConfigureAwait(false);
            cancellationToken.ThrowIfCancellationRequested();
            return await manager.IsTrusted(certificateFileOrSignedFile, cancellationToken).ConfigureAwait(false);
        }

        public async Task<List<PersonalCertificate>> GetCertificatesFromStore(CertificateStoreType certificateStoreType, bool onlyValid = true, CancellationToken cancellationToken = default, IProgress<ProgressData> progress = null)
        {
            var manager = await this.managerFactory.GetProxyFor(SelfElevationLevel.AsInvoker, cancellationToken).ConfigureAwait(false);
            cancellationToken.ThrowIfCancellationRequested();
            return await manager.GetCertificatesFromStore(certificateStoreType, onlyValid, cancellationToken, progress).ConfigureAwait(false);
        }

        public async Task ExtractCertificateFromMsix(string msixFile, string outputFile, CancellationToken cancellationToken = default, IProgress<ProgressData> progress = null)
        {
            var manager = await this.managerFactory.GetProxyFor(SelfElevationLevel.AsInvoker, cancellationToken).ConfigureAwait(false);
            cancellationToken.ThrowIfCancellationRequested();
            await manager.ExtractCertificateFromMsix(msixFile, outputFile, cancellationToken, progress).ConfigureAwait(false);
        }

        public async Task ExtractCertificateFromMsix(string msixFile, CancellationToken cancellationToken = default, IProgress<ProgressData> progress = null)
        {
            var manager = await this.managerFactory.GetProxyFor(SelfElevationLevel.AsInvoker, cancellationToken).ConfigureAwait(false);
            cancellationToken.ThrowIfCancellationRequested();
            await manager.ExtractCertificateFromMsix(msixFile, cancellationToken, progress).ConfigureAwait(false);
        }

        public async Task<PersonalCertificate> GetCertificateFromMsix(string msixFile, CancellationToken cancellationToken = default, IProgress<ProgressData> progress = null)
        {
            var manager = await this.managerFactory.GetProxyFor(SelfElevationLevel.AsInvoker, cancellationToken).ConfigureAwait(false);
            cancellationToken.ThrowIfCancellationRequested();
            return await manager.GetCertificateFromMsix(msixFile, cancellationToken, progress).ConfigureAwait(false);
        }

        public async Task SignPackageWithPfx(string package, bool updatePublisher, string pfxPath, SecureString password, string timestampUrl = null, IncreaseVersionMethod increaseVersion = IncreaseVersionMethod.None, CancellationToken cancellationToken = default, IProgress<ProgressData> progress = null)
        {
            var manager = await this.managerFactory.GetProxyFor(SelfElevationLevel.AsInvoker, cancellationToken).ConfigureAwait(false);
            cancellationToken.ThrowIfCancellationRequested();
            await manager.SignPackageWithPfx(package, updatePublisher, pfxPath, password, timestampUrl, increaseVersion, cancellationToken, progress).ConfigureAwait(false);
        }

        public async Task SignPackageWithDeviceGuard(string package,
            bool updatePublisher,
            DeviceGuardConfig config,
            string timestampUrl = null,
            IncreaseVersionMethod increaseVersion = IncreaseVersionMethod.None,
            CancellationToken cancellationToken = default,
            IProgress<ProgressData> progress = null)
        {
            var manager = await this.managerFactory.GetProxyFor(SelfElevationLevel.AsInvoker, cancellationToken).ConfigureAwait(false);
            cancellationToken.ThrowIfCancellationRequested();
            await manager.SignPackageWithDeviceGuard(package, updatePublisher, config, timestampUrl, increaseVersion, cancellationToken, progress).ConfigureAwait(false);
        }

        public async Task SignPackageWithInstalled(string package, bool updatePublisher, PersonalCertificate certificate, string timestampUrl = null, IncreaseVersionMethod increaseVersion = IncreaseVersionMethod.None, CancellationToken cancellationToken = default, IProgress<ProgressData> progress = null)
        {
            var manager = await this.managerFactory.GetProxyFor(SelfElevationLevel.AsInvoker, cancellationToken).ConfigureAwait(false);
            cancellationToken.ThrowIfCancellationRequested();
            await manager.SignPackageWithInstalled(package, updatePublisher, certificate, timestampUrl, increaseVersion, cancellationToken, progress).ConfigureAwait(false);
        }
    }
}