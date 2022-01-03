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
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security;
using System.Threading.Tasks;
using CommandLine;
using Newtonsoft.Json.Linq;
using Otor.MsixHero.Appx.Signing;
using Otor.MsixHero.Appx.Signing.DeviceGuard;
using Otor.MsixHero.Appx.Signing.Entities;
using Otor.MsixHero.Cli.Helpers;
using Otor.MsixHero.Cli.Verbs;
using Otor.MsixHero.Infrastructure.Configuration;
using Otor.MsixHero.Infrastructure.Cryptography;
using Otor.MsixHero.Infrastructure.Helpers;
using Otor.MsixHero.Infrastructure.Logging;
using Otor.MsixHero.Infrastructure.Services;
using Otor.MsixHero.Infrastructure.ThirdParty.Exceptions;

namespace Otor.MsixHero.Cli.Executors.Standard
{
    public class SignVerbExecutor : VerbExecutor<SignVerb>
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(SignVerbExecutor));
        private readonly ISigningManager signingManager;
        private readonly IConfigurationService configurationService;
        protected readonly DeviceGuardHelper DeviceGuardHelper = new DeviceGuardHelper();
        protected readonly DgssTokenCreator DeviceGuardTokenCreator = new DgssTokenCreator();

        public SignVerbExecutor(SignVerb signVerb, ISigningManager signingManager, IConfigurationService configurationService, IConsole console) : base(signVerb, console)
        {
            this.signingManager = signingManager;
            this.configurationService = configurationService;
        }

        public override async Task<int> Execute()
        {
            var checkCmd = await this.AssertCorrectCommandLine().ConfigureAwait(false);
            if (checkCmd != StandardExitCodes.ErrorSuccess)
            {
                return checkCmd;
            }

            var config = await configurationService.GetCurrentConfigurationAsync().ConfigureAwait(false);

            if (config.Signing?.Source == CertificateSource.Unknown)
            {
                // workaround for some migration issues
                if (!string.IsNullOrEmpty(config.Signing.PfxPath))
                {
                    config.Signing.Source = CertificateSource.Pfx;
                }
                else if (!string.IsNullOrEmpty(config.Signing.Thumbprint))
                {
                    config.Signing.Source = CertificateSource.Personal;
                }
                else
                {
                    await this.Console.WriteError("In order to sign with CLI without any extra signing parameters, configure your signature in MSIX Hero settings.").ConfigureAwait(false);
                    return 1;
                }
            }

            if (this.Verb.ThumbPrint != null)
            {
                return await this.SignStore(
                    this.Verb.ThumbPrint, 
                    this.Verb.TimeStampUrl ?? config.Signing?.TimeStampServer,
                    !this.Verb.NoPublisherUpdate).ConfigureAwait(false);
            }

            if (this.Verb.PfxFilePath != null)
            {
                return await this.SignPfx(
                    this.Verb.PfxFilePath, 
                    this.Verb.PfxPassword, 
                    this.Verb.TimeStampUrl ?? config.Signing?.TimeStampServer,
                    !this.Verb.NoPublisherUpdate).ConfigureAwait(false);
            }

            if (this.Verb.DeviceGuardInteractive)
            {
                return await this.SignDeviceGuardInteractive(
                    this.Verb.TimeStampUrl ?? config.Signing?.TimeStampServer,
                    !this.Verb.NoPublisherUpdate).ConfigureAwait(false);
            }

            if (this.Verb.DeviceGuardFile != null)
            {
                var json = JObject.Parse(await File.ReadAllTextAsync(this.Verb.DeviceGuardFile).ConfigureAwait(false));
                var cfg = new DeviceGuardConfig
                {
                    AccessToken = json["access_token"]?.Value<string>(),
                    RefreshToken = json["access_token"]?.Value<string>(),
                    Subject = this.Verb.DeviceGuardSubject
                };

                if (cfg.Subject == null && !this.Verb.NoPublisherUpdate)
                {
                    await this.Console.WriteInfo("Determining publisher name from Device Guard Signing certificate...").ConfigureAwait(false);
                    cfg.Subject = await this.DeviceGuardHelper.GetSubjectFromDeviceGuardSigning(cfg.AccessToken, cfg.RefreshToken);
                    await this.Console.WriteSuccess("New publisher name is " + cfg.Subject).ConfigureAwait(false);
                }

                return await this.SignDeviceGuard(
                    cfg, 
                    this.Verb.TimeStampUrl ?? config.Signing?.TimeStampServer,
                    !this.Verb.NoPublisherUpdate).ConfigureAwait(false);
            }

            await this.Console.WriteInfo("Using current MSIX Hero signing options...").ConfigureAwait(false);

            switch (config?.Signing?.Source)
            {
                case CertificateSource.Pfx:
                    string password = null;

                    if (!string.IsNullOrEmpty(config.Signing?.EncodedPassword))
                    {
                        var crypto = new Crypto();
                        try
                        {
                            password = crypto.DecryptString(config.Signing.EncodedPassword, "$%!!ASddahs55839AA___ąółęńśSdcvv");
                        }
                        catch (Exception)
                        {
                            Logger.Error("Could not use the configured password. Decryption of the string from settings failed.");
                            await this.Console.WriteError("Could not use the configured password. Decryption of the string from settings failed.").ConfigureAwait(false);
                            return StandardExitCodes.ErrorSettings;
                        }
                    }
                        
                    return await this.SignPfx(
                        config.Signing.PfxPath.Resolved, 
                        password, 
                        this.Verb.TimeStampUrl ?? config.Signing?.TimeStampServer,
                        !this.Verb.NoPublisherUpdate).ConfigureAwait(false);
                case CertificateSource.Personal:
                    return await this.SignStore(
                        config.Signing.Thumbprint, 
                        this.Verb.TimeStampUrl ?? config.Signing?.TimeStampServer,
                        !this.Verb.NoPublisherUpdate);
                case CertificateSource.DeviceGuard:
                    if (config.Signing.DeviceGuard == null)
                    {
                        Logger.Error("Device Guard has not been configured yet.");
                        await this.Console.WriteError("Device Guard has not been configured yet.").ConfigureAwait(false);
                        return StandardExitCodes.ErrorSettings;
                    }

                    return await this.SignDeviceGuard(
                        config.Signing.DeviceGuard.FromConfiguration(), 
                        this.Verb.TimeStampUrl ?? config.Signing?.TimeStampServer,
                        !this.Verb.NoPublisherUpdate);
                default:
                    Logger.Error("No certificate has been provided, and no default certificate has been configured in MSIX Hero settings.");
                    await this.Console.WriteError("No certificate has been provided, and no default certificate has been configured in MSIX Hero settings.").ConfigureAwait(false);
                    return StandardExitCodes.ErrorSettings;
            }
        }

        private static string GetOptionName(string propertyName)
        {
            var property = typeof(SignVerb).GetProperty(propertyName);
            if (property == null)
            {
                return null;
            }

            var customAttrs = (OptionAttribute)property.GetCustomAttribute(typeof(OptionAttribute));
            if (customAttrs == null)
            {
                return null;
            }

            return customAttrs.LongName ?? customAttrs.ShortName;
        }

        private async Task<int> GetSetError(string prop1, string prop2)
        {
            var msg = $"Options --{GetOptionName(prop1)} and --{GetOptionName(prop2)} cannot be used together.";
            await this.Console.WriteError(msg).ConfigureAwait(false);
            Logger.Error(msg);
            return (int)ErrorType.MutuallyExclusiveSetError;
        }

        private async Task<int> GetSetValueError(string property, string message)
        {
            var msg = $"Option --{GetOptionName(property)}: {message}";
            await this.Console.WriteError(msg).ConfigureAwait(false);
            Logger.Error(msg);
            return (int)ErrorType.SetValueExceptionError;
        }

        private async Task<int> AssertCorrectCommandLine()
        {
            if (this.Verb.ThumbPrint != null)
            {
                if (this.Verb.PfxFilePath != null)
                {
                    return await this.GetSetError(nameof(SignVerb.ThumbPrint), nameof(SignVerb.PfxFilePath));
                }

                if (this.Verb.PfxPassword != null)
                {
                    return await this.GetSetError(nameof(SignVerb.ThumbPrint), nameof(SignVerb.PfxPassword));
                }

                if (this.Verb.DeviceGuardInteractive)
                {
                    return await this.GetSetError(nameof(SignVerb.ThumbPrint), nameof(SignVerb.DeviceGuardInteractive));
                }

                if (this.Verb.DeviceGuardFile != null)
                {
                    return await this.GetSetError(nameof(SignVerb.ThumbPrint), nameof(SignVerb.DeviceGuardFile));
                }

                if (this.Verb.DeviceGuardSubject != null)
                {
                    return await this.GetSetError(nameof(SignVerb.ThumbPrint), nameof(SignVerb.DeviceGuardSubject));
                }
            }

            if (this.Verb.PfxFilePath != null)
            {
                if (this.Verb.DeviceGuardInteractive)
                {
                    return await this.GetSetError(nameof(SignVerb.PfxFilePath), nameof(SignVerb.DeviceGuardInteractive));
                }

                if (this.Verb.DeviceGuardFile != null)
                {
                    return await this.GetSetError(nameof(SignVerb.PfxFilePath), nameof(SignVerb.DeviceGuardFile));
                }

                if (this.Verb.DeviceGuardSubject != null)
                {
                    return await this.GetSetError(nameof(SignVerb.PfxFilePath), nameof(SignVerb.DeviceGuardSubject));
                }

                if (this.Verb.UseMachineStore)
                {
                    return await this.GetSetError(nameof(SignVerb.PfxFilePath), nameof(SignVerb.UseMachineStore));
                }
            }

            if (this.Verb.DeviceGuardInteractive)
            {
                if (this.Verb.DeviceGuardFile != null)
                {
                    return await this.GetSetError(nameof(SignVerb.DeviceGuardInteractive), nameof(SignVerb.DeviceGuardFile));
                }

                if (this.Verb.DeviceGuardSubject != null)
                {
                    return await this.GetSetError(nameof(SignVerb.DeviceGuardInteractive), nameof(SignVerb.DeviceGuardSubject));
                }

                if (this.Verb.UseMachineStore)
                {
                    return await this.GetSetError(nameof(SignVerb.DeviceGuardInteractive), nameof(SignVerb.UseMachineStore));
                }
            }

            if (this.Verb.DeviceGuardFile != null)
            {
                if (this.Verb.UseMachineStore)
                {
                    return await this.GetSetError(nameof(SignVerb.DeviceGuardFile), nameof(SignVerb.UseMachineStore));
                }
            }

            if (this.Verb.DeviceGuardSubject != null)
            {
                if (this.Verb.UseMachineStore)
                {
                    return await this.GetSetError(nameof(SignVerb.DeviceGuardSubject), nameof(SignVerb.UseMachineStore));
                }
            }

            if (this.Verb.PfxFilePath != null && !File.Exists(this.Verb.PfxFilePath))
            {
                return await this.GetSetValueError(nameof(SignVerb.PfxFilePath), $"The file '{this.Verb.PfxFilePath}' does not exist.");
            }

            if (this.Verb.DeviceGuardFile != null && !File.Exists(this.Verb.DeviceGuardFile))
            {
                return await this.GetSetValueError(nameof(SignVerb.DeviceGuardFile), $"The file '{this.Verb.PfxFilePath}' does not exist.");
            }

            if (this.Verb.TimeStampUrl != null && !Uri.TryCreate(this.Verb.TimeStampUrl, UriKind.Absolute, out _))
            {
                return await this.GetSetValueError(nameof(SignVerb.TimeStampUrl), $"The value '{this.Verb.TimeStampUrl}' is not a valid URI.");
            }

            return StandardExitCodes.ErrorSuccess;
        }

        private async Task<int> SignDeviceGuard(DeviceGuardConfig cfg, string timestamp, bool updatePublisherName)
        {
            try
            {
                foreach (var path in this.Verb.FilePath)
                {
                    await this.Console.WriteInfo($"Signing '{path}' with Device Guard...");
                    await this.signingManager.SignPackageWithDeviceGuard(path, updatePublisherName, cfg, timestamp, this.Verb.IncreaseVersion).ConfigureAwait(false);
                    await this.Console.WriteSuccess("Package signed successfully!").ConfigureAwait(false);
                    await this.Console.ShowCertSummary(signingManager, path);
                }

                return StandardExitCodes.ErrorSuccess;
            }
            catch (SdkException e)
            {
                Logger.Error(e);
                await this.Console.WriteError($"Signing failed with error code 0x{e.ExitCode:X}: {e.Message}");
                return e.ExitCode;
            }
            catch (Exception e)
            {
                Logger.Error(e);
                await this.Console.WriteError($"Signing failed: {e.Message}");
                return StandardExitCodes.ErrorGeneric;
            }
        }

        private async Task<int> SignDeviceGuardInteractive(string timestamp, bool updatePublisherName)
        {
            string json = null;
            DeviceGuardConfig cfg;

            try
            {
                cfg = await this.DeviceGuardTokenCreator.SignIn().ConfigureAwait(false);
                json = await this.DeviceGuardTokenCreator.CreateDeviceGuardJsonTokenFile(cfg).ConfigureAwait(false);

                if (!this.Verb.NoPublisherUpdate)
                {
                    await this.Console.WriteInfo("Determining publisher name from Device Guard Signing certificate...").ConfigureAwait(false);
                    cfg.Subject = await this.DeviceGuardHelper.GetSubjectFromDeviceGuardSigning(json).ConfigureAwait(false);
                    await this.Console.WriteSuccess("New publisher name is " + cfg.Subject).ConfigureAwait(false);
                }
            }
            catch (SdkException e)
            {
                Logger.Error(e);
                await this.Console.WriteError($"Signing failed with error code 0x{e.ExitCode:X}: {e.Message}");
                return e.ExitCode;
            }
            catch (Exception e)
            {
                Logger.Error(e);
                await this.Console.WriteError($"Signing failed: {e.Message}");
                return StandardExitCodes.ErrorGeneric;
            }
            finally
            {
                if (json != null && File.Exists(json))
                {
                    ExceptionGuard.Guard(() => File.Delete(json));
                }
            }

            return await this.SignDeviceGuard(cfg, timestamp, updatePublisherName).ConfigureAwait(false);
        }

        private async Task<int> SignPfx(string pfxPath, string password, string timestamp, bool updatePublisherName)
        {
            try
            {
                using var secPass = new SecureString();
                foreach (var passChar in password ?? String.Empty)
                {
                    secPass.AppendChar(passChar);
                }

                var fileName = Path.GetFileName(pfxPath);
                foreach (var path in this.Verb.FilePath)
                {
                    await this.Console.WriteInfo($"Signing '{path}' with certificate [{fileName}]...");
                    await this.signingManager.SignPackageWithPfx(path, updatePublisherName, pfxPath, secPass, timestamp, this.Verb.IncreaseVersion).ConfigureAwait(false);
                    await this.Console.WriteSuccess("Package signed successfully!").ConfigureAwait(false);
                    await this.Console.ShowCertSummary(signingManager, path);
                }

                return StandardExitCodes.ErrorSuccess;
            }
            catch (Exception e)
            {
                Logger.Error(e);
                await this.Console.WriteError($"Signing failed: {e.Message}");
                return StandardExitCodes.ErrorGeneric;
            }
        }

        private async Task<int> SignStore(string thumbprint, string timestamp, bool updatePublisherName)
        {
            var mode = this.Verb.UseMachineStore ? CertificateStoreType.Machine : CertificateStoreType.User;
            var certificates = await this.signingManager.GetCertificatesFromStore(mode, false).ConfigureAwait(false);

            var certificate = certificates.FirstOrDefault(c => string.Equals(c.Thumbprint, thumbprint, StringComparison.Ordinal));
            if (certificate == null)
            {
                Logger.Error("Certificate with thumbprint {0} was not found.", thumbprint);
                await this.Console.WriteError($"Certificate with thumbprint {thumbprint} could not be found");
                return StandardExitCodes.ErrorGeneric;
            }

            try
            {
                foreach (var path in this.Verb.FilePath)
                {
                    await this.Console.WriteInfo($"Signing '{path}' with certificate [SHA1 = {thumbprint}]...");
                    await this.signingManager.SignPackageWithInstalled(path, updatePublisherName, certificate, timestamp, this.Verb.IncreaseVersion).ConfigureAwait(false);
                    await this.Console.WriteSuccess("Package signed successfully!").ConfigureAwait(false);
                    await this.Console.ShowCertSummary(signingManager, path);
                }

                return StandardExitCodes.ErrorSuccess;
            }
            catch (SdkException e)
            {
                Logger.Error(e);
                await this.Console.WriteError($"Signing failed with error code 0x{e.ExitCode:X}: {e.Message}");
                return e.ExitCode;
            }
            catch (Exception e)
            {
                Logger.Error(e);
                await this.Console.WriteError($"Signing failed: {e.Message}");
                return StandardExitCodes.ErrorGeneric;
            }
        }
    }
}