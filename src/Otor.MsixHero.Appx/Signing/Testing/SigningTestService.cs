// MSIX Hero
// Copyright (C) 2024 Marcin Otorowski
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

using System.IO;
using System.Linq;
using System;
using System.Security;
using System.Threading;
using System.Threading.Tasks;
using Otor.MsixHero.Appx.Signing.DeviceGuard;
using Otor.MsixHero.Infrastructure.ThirdParty.Sdk;
using Dapplo.Log;
using Otor.MsixHero.Infrastructure.Helpers;
using System.Security.Cryptography.X509Certificates;
using Otor.MsixHero.Infrastructure.Configuration;
using Otor.MsixHero.Infrastructure.Cryptography;
using System.Runtime.InteropServices;

namespace Otor.MsixHero.Appx.Signing.Testing;

public class SigningTestService : ISigningTestService
{
    private readonly ISigningManager _signingManager;
    private static readonly LogSource Logger = new();

    public SigningTestService(ISigningManager signingManager)
    {
        this._signingManager = signingManager;
    }

    public async Task<SignTestResult> VerifyPfx(string pfxPath, SecureString password = default, string timeStampServer = default, CancellationToken cancellation = default)
    {
        string tmpFile = null;
        try
        {
            tmpFile = await this.GetCandidateForSigning(cancellation).ConfigureAwait(false);

            var signTool = new SignToolWrapper();
            var x509 = new X509Certificate2(await File.ReadAllBytesAsync(pfxPath, cancellation).ConfigureAwait(false), password);

            string type;
            if (x509.SignatureAlgorithm.FriendlyName?.EndsWith("rsa", StringComparison.OrdinalIgnoreCase) == true)
            {
                type = x509.SignatureAlgorithm.FriendlyName.Substring(0, x509.SignatureAlgorithm.FriendlyName.Length - 3).ToUpperInvariant();
            }
            else
            {
                return new SignTestResult(string.Format(Resources.Localization.Signing_Test_AlgorithmNotSupported_Format, x509.SignatureAlgorithm.FriendlyName), SignTestResultType.Error);
            }

            var openTextPassword = new System.Net.NetworkCredential(string.Empty, password).Password;
            await signTool.SignWithPfx(new[] { tmpFile }, type, pfxPath, openTextPassword, timeStampServer, cancellation).ConfigureAwait(false);

            var isTrusted = await this._signingManager.IsTrusted(tmpFile, cancellation).ConfigureAwait(false);
            if (!isTrusted.IsTrusted)
            {
                return new SignTestResult(string.Format(Resources.Localization.Signing_Test_OK_NotTrusted_Format, Path.GetFileName(pfxPath)), SignTestResultType.Warn);
            }

            return new SignTestResult(null, SignTestResultType.Ok);
        }
        catch (Exception e)
        {
            if (e.HResult == -2147024810)
            {
                return new SignTestResult(Resources.Localization.Signing_WrongPassword, SignTestResultType.Error);
            }

            return new SignTestResult(e.GetBaseException().Message, SignTestResultType.Error);
        }
        finally
        {
            if (tmpFile != null && File.Exists(tmpFile))
            {
                ExceptionGuard.Guard(() => File.Delete(tmpFile));
            }
        }
    }

    public async Task<SignTestResult> VerifyInstalled(string thumbprint, string timeStampServer = default, CancellationToken cancellation = default)
    {
        string tmpFile = null;
        try
        {
            tmpFile = await this.GetCandidateForSigning(cancellation).ConfigureAwait(false);
            var stores = new[] { StoreLocation.CurrentUser, StoreLocation.LocalMachine };

            string lastError = null;
            X509Certificate2Collection x509 = default;

            var useMachineStore = false;
            foreach (var loc in stores)
            {
                using var store = new X509Store(StoreName.My, loc);
                store.Open(OpenFlags.ReadOnly | OpenFlags.OpenExistingOnly);

                x509 = store.Certificates.Find(X509FindType.FindByThumbprint, thumbprint, false);
                if (x509.Count < 1)
                {
                    continue;
                }

                var isForCodeSigning = x509[0].Extensions.OfType<X509KeyUsageExtension>().Any(ke => ke.KeyUsages.HasFlag(X509KeyUsageFlags.DigitalSignature));

                if (!isForCodeSigning)
                {
                    lastError = Resources.Localization.Signing_Test_MissingKeyUsage;
                    continue;
                }

                if (!x509[0].HasPrivateKey)
                {
                    lastError = Resources.Localization.Signing_Test_NoPrivateKey;
                    continue;
                }

                useMachineStore = loc == StoreLocation.LocalMachine;
                break;
            }

            if (x509?.Any() != true)
            {
                if (lastError != null)
                {
                    return new SignTestResult(lastError, SignTestResultType.Error);
                }

                return new SignTestResult(string.Format(Resources.Localization.Signing_Test_WrongThumbPrint_Format, thumbprint), SignTestResultType.Error);
            }

            string type;
            if (x509[0].SignatureAlgorithm.FriendlyName?.EndsWith("rsa", StringComparison.OrdinalIgnoreCase) == true)
            {
                type = x509[0].SignatureAlgorithm.FriendlyName?.Substring(0, x509[0].SignatureAlgorithm.FriendlyName.Length - 3).ToUpperInvariant();
            }
            else
            {
                return new SignTestResult(string.Format(Resources.Localization.Signing_Test_AlgorithmNotSupported_Format, x509[0].SignatureAlgorithm.FriendlyName), SignTestResultType.Error);
            }

            var signTool = new SignToolWrapper();
            await signTool.SignWithPersonal(new[] { tmpFile }, type, thumbprint, useMachineStore, timeStampServer, cancellation).ConfigureAwait(false);

            var isTrusted = await this._signingManager.IsTrusted(tmpFile, cancellation).ConfigureAwait(false);
            if (!isTrusted.IsTrusted)
            {
                return new SignTestResult(string.Format(Resources.Localization.Signing_Test_OK_NotTrusted_Format, x509[0].FriendlyName), SignTestResultType.Warn);
            }

            return new SignTestResult(null, SignTestResultType.Ok);
        }
        catch (Exception e)
        {
            return new SignTestResult(e.GetBaseException().Message, SignTestResultType.Error);
        }
        finally
        {
            if (tmpFile != null && File.Exists(tmpFile))
            {
                ExceptionGuard.Guard(() => File.Delete(tmpFile));
            }
        }
    }

    public async Task<SignTestResult> VerifyDeviceGuardSettings(DeviceGuardConfiguration config, string timeStampServer = default, CancellationToken cancellation = default)
    {
        if (config.EncodedRefreshToken == null || config.Subject == null)
        {
            return new SignTestResult(Resources.Localization.Signing_Test_DeviceGuardConfigMissing, SignTestResultType.Error);
        }

        var tokens = new DeviceGuardConfig();
        var crypto = new Crypto();
        tokens.AccessToken = FromSecureString(crypto.Unprotect(config.EncodedAccessToken));
        tokens.RefreshToken = FromSecureString(crypto.Unprotect(config.EncodedRefreshToken));
        tokens.Subject = config.Subject;

        var dgssTokenPath = await new DgssTokenCreator().CreateDeviceGuardJsonTokenFile(tokens, cancellation).ConfigureAwait(false);
        var tempFile = await GetCandidateForSigning(cancellation).ConfigureAwait(false);

        try
        {
            var signWrapper = new SignToolWrapper();
            await signWrapper.SignWithDeviceGuard(new[] { tempFile }, "SHA256", dgssTokenPath, timeStampServer, cancellation).ConfigureAwait(false);

            var isTrusted = await this._signingManager.IsTrusted(tempFile, cancellation).ConfigureAwait(false);
            if (!isTrusted.IsTrusted)
            {
                return new SignTestResult(Resources.Localization.Signing_Test_OK_NotTrustedDG, SignTestResultType.Warn);
            }

            return new SignTestResult(null, SignTestResultType.Ok);
        }
        finally
        {
            if (tempFile != null && File.Exists(tempFile))
            {
                ExceptionGuard.Guard(() => File.Delete(tempFile));
            }

            if (dgssTokenPath != null && File.Exists(dgssTokenPath))
            {
                ExceptionGuard.Guard(() => File.Delete(dgssTokenPath));
            }
        }
    }

    private async Task<string> GetCandidateForSigning(CancellationToken cancellationToken)
    {
        var tempFilePath = Path.Combine(Path.GetTempPath(), "msix-hero-" + Guid.NewGuid().ToString("N") + ".cat");

        var name = typeof(DeviceGuardHelper).Assembly.GetManifestResourceNames().First(n => n.EndsWith("MSIXHeroTest.cat"));
        await using var manifestResourceStream = typeof(DeviceGuardHelper).Assembly.GetManifestResourceStream(name);
        if (manifestResourceStream == null)
        {
            throw new InvalidOperationException(Resources.Localization.Signing_Test_InternalError);
        }

        Logger.Debug().WriteLine($"Creating temporary file path {tempFilePath}");
        await using var fileStream = File.Create(tempFilePath);
        manifestResourceStream.Seek(0L, SeekOrigin.Begin);
        await manifestResourceStream.CopyToAsync(fileStream, cancellationToken).ConfigureAwait(false);

        return tempFilePath;
    }

    private static string FromSecureString(SecureString secureString)
    {
        var unmanagedString = IntPtr.Zero;
        try
        {
            unmanagedString = Marshal.SecureStringToGlobalAllocUnicode(secureString);
            // ReSharper disable once AssignNullToNotNullAttribute
            return Marshal.PtrToStringUni(unmanagedString);
        }
        finally
        {
            if (unmanagedString != IntPtr.Zero)
            {
                Marshal.ZeroFreeGlobalAllocUnicode(unmanagedString);
            }
        }
    }
}