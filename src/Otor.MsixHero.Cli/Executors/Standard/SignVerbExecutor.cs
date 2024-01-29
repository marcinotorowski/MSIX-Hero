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

using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security;
using System.Threading;
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
using Dapplo.Log;
using Otor.MsixHero.Infrastructure.Services;
using Otor.MsixHero.Infrastructure.ThirdParty.Exceptions;

namespace Otor.MsixHero.Cli.Executors.Standard
{
    public class SignVerbExecutor : VerbExecutor<SignVerb>
    {
        private static readonly LogSource Logger = new();        
        private readonly ISigningManager _signingManager;
        private readonly IConfigurationService _configurationService;
        protected readonly DeviceGuardHelper DeviceGuardHelper = new();
        protected readonly DgssTokenCreator DeviceGuardTokenCreator = new();

        public SignVerbExecutor(SignVerb signVerb, ISigningManager signingManager, IConfigurationService configurationService, IConsole console) : base(signVerb, console)
        {
            this._signingManager = signingManager;
            this._configurationService = configurationService;
        }

        public override async Task<int> Execute()
        {
            var checkCmd = await this.AssertCorrectCommandLine().ConfigureAwait(false);
            if (checkCmd != StandardExitCodes.ErrorSuccess)
            {
                return checkCmd;
            }

            var config = await _configurationService.GetCurrentConfigurationAsync().ConfigureAwait(false);

            // Signing with thumbprint
            if (this.Verb.ThumbPrint != null)
            {
                return await this.SignStore(
                    this.Verb.ThumbPrint, 
                    this.Verb.TimeStampUrl ?? config.Signing?.TimeStampServer,
                    !this.Verb.NoPublisherUpdate).ConfigureAwait(false);
            }

            // Signing with PFX
            if (this.Verb.PfxFilePath != null)
            {
                return await this.SignPfx(
                    this.Verb.PfxFilePath, 
                    this.Verb.PfxPassword, 
                    this.Verb.TimeStampUrl ?? config.Signing?.TimeStampServer,
                    !this.Verb.NoPublisherUpdate).ConfigureAwait(false);
            }

            // Signing with Device Guard (interactive)
            if (this.Verb.DeviceGuardInteractive)
            {
                return await this.SignDeviceGuardInteractive(
                    this.Verb.TimeStampUrl ?? config.Signing?.TimeStampServer, !this.Verb.NoPublisherUpdate).ConfigureAwait(false);
            }

            // Signing with Device Guard
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
                    await this.Console.WriteInfo(Resources.Localization.CLI_Executor_Sign_DeviceGuardDeterminingPublisher).ConfigureAwait(false);
                    cfg.Subject = await this.DeviceGuardHelper.GetSubjectFromDeviceGuardSigning(cfg.AccessToken, cfg.RefreshToken);
                    await this.Console.WriteSuccess(string.Format(Resources.Localization.CLI_Executor_Sign_NewName_Format, cfg.Subject)).ConfigureAwait(false);
                }

                return await this.SignDeviceGuard(
                    cfg, 
                    this.Verb.TimeStampUrl ?? config.Signing?.TimeStampServer,
                    !this.Verb.NoPublisherUpdate).ConfigureAwait(false);
            }

            // Fallback - try to get MSIX Hero default settings
            return await this.SignDefault().ConfigureAwait(false);
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
            var msg = string.Format(Resources.Localization.CLI_Executor_Sign_Error_OptionConflict_Format, GetOptionName(prop1), GetOptionName(prop2));
            await this.Console.WriteError(msg).ConfigureAwait(false);
            Logger.Error().WriteLine(msg);
            return (int)ErrorType.MutuallyExclusiveSetError;
        }

        private async Task<int> GetSetValueError(string property, string message)
        {
            var msg = string.Format(Resources.Localization.CLI_Executor_Sign_Error_Option_Format, GetOptionName(property), message);
            await this.Console.WriteError(msg).ConfigureAwait(false);
            Logger.Error().WriteLine(msg);
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
                return await this.GetSetValueError(nameof(SignVerb.PfxFilePath), string.Format(Resources.Localization.CLI_Error_FileNotExists_Format, this.Verb.PfxFilePath));
            }

            if (this.Verb.DeviceGuardFile != null && !File.Exists(this.Verb.DeviceGuardFile))
            {
                return await this.GetSetValueError(nameof(SignVerb.DeviceGuardFile), string.Format(Resources.Localization.CLI_Error_FileNotExists_Format, this.Verb.DeviceGuardFile));
            }

            if (this.Verb.TimeStampUrl != null && !Uri.TryCreate(this.Verb.TimeStampUrl, UriKind.Absolute, out _))
            {
                return await this.GetSetValueError(nameof(SignVerb.TimeStampUrl), string.Format(Resources.Localization.CLI_Executor_Sign_Error_InvalidTimestampUrl_Format, this.Verb.TimeStampUrl));
            }

            return StandardExitCodes.ErrorSuccess;
        }
        
        private async Task<int> SignDefault()
        {
            var config = await this._configurationService.GetCurrentConfigurationAsync().ConfigureAwait(false);

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
                    await this.Console.WriteError(Resources.Localization.CLI_Executor_Sign_Error_NoConfig).ConfigureAwait(false);
                    return 1;
                }
            }

            await this.Console.WriteInfo(Resources.Localization.CLI_Executor_Sign_UsingCurrent).ConfigureAwait(false);

            switch (config.Signing?.Source)
            {
                case CertificateSource.Pfx:
                    string password = null;

                    if (!string.IsNullOrEmpty(config.Signing?.EncodedPassword))
                    {
                        var crypto = new Crypto();

                        try
                        {
                            password = crypto.UnprotectUnsafe(config.Signing?.EncodedPassword);
                        }
                        catch
                        {
                            Logger.Warn().WriteLine("It seems that your are using the old-way of protecting password. MSIX Hero will try to use the legacy method now, but consider updating your settings so that the password will be safely encrypted.");
                            await this.Console.WriteWarning("Could not use the configured password. Decryption of the string from settings failed.").ConfigureAwait(false);

                            try
                            {
                                // ReSharper disable StringLiteralTypo
#pragma warning disable CS0618
                                password = crypto.DecryptString(config.Signing?.EncodedPassword, @"$%!!ASddahs55839AA___ąółęńśSdcvv");
#pragma warning restore CS0618
                                // ReSharper restore StringLiteralTypo
                            }
                            catch (Exception)
                            {
                                Logger.Error().WriteLine(Resources.Localization.CLI_Executor_Sign_Error_DecryptFailed);
                                await this.Console.WriteError(Resources.Localization.CLI_Executor_Sign_Error_DecryptFailed).ConfigureAwait(false);
                                return StandardExitCodes.ErrorSettings;
                            }
                        }
                    }
                    return await this.SignPfx(config.Signing?.PfxPath?.Resolved, password, this.Verb.TimeStampUrl ?? config.Signing?.TimeStampServer, !this.Verb.NoPublisherUpdate).ConfigureAwait(false);

                case CertificateSource.Personal:
                    return await this.SignStore(config.Signing.Thumbprint, this.Verb.TimeStampUrl ?? config.Signing?.TimeStampServer, !this.Verb.NoPublisherUpdate);
                
                case CertificateSource.DeviceGuard:
                    if (config.Signing.DeviceGuard == null)
                    {
                        Logger.Error().WriteLine(Resources.Localization.CLI_Executor_Sign_Error_DeviceGuardNoConfig);
                        await this.Console.WriteError(Resources.Localization.CLI_Executor_Sign_Error_DeviceGuardNoConfig).ConfigureAwait(false);
                        return StandardExitCodes.ErrorSettings;
                    }

                    return await this.SignDeviceGuard(config.Signing.DeviceGuard.FromConfiguration(), this.Verb.TimeStampUrl ?? config.Signing?.TimeStampServer, !this.Verb.NoPublisherUpdate);

                default:
                    Logger.Error().WriteLine(Resources.Localization.CLI_Executor_Sign_Error_NoCertAndDefaultConfig);
                    await this.Console.WriteError(Resources.Localization.CLI_Executor_Sign_Error_NoCertAndDefaultConfig).ConfigureAwait(false);
                    return StandardExitCodes.ErrorSettings;
            }
        }

        private async Task<int> SignDeviceGuard(DeviceGuardConfig cfg, string timestamp, bool updatePublisherName)
        {
            try
            {
                foreach (var path in this.Verb.FilePath)
                {
                    await this.Console.WriteInfo(string.Format(Resources.Localization.CLI_Executor_Sign_DeviceGuardSigning_Format, path));
                    await this._signingManager.SignPackageWithDeviceGuard(path, updatePublisherName, cfg, timestamp, this.Verb.IncreaseVersion).ConfigureAwait(false);
                    await this.Console.WriteSuccess(Resources.Localization.CLI_Executor_Sign_Success).ConfigureAwait(false);
                    await this.Console.ShowCertSummary(this._signingManager, path);
                }

                return StandardExitCodes.ErrorSuccess;
            }
            catch (SdkException e)
            {
                Logger.Error().WriteLine(e);
                await this.Console.WriteError($"Signing failed with error code 0x{e.ExitCode:X}: {e.Message}");
                return e.ExitCode;
            }
            catch (Exception e)
            {
                Logger.Error().WriteLine(e);
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
                cfg = await this.DeviceGuardTokenCreator.SignIn(false, CancellationToken.None).ConfigureAwait(false);
                json = await this.DeviceGuardTokenCreator.CreateDeviceGuardJsonTokenFile(cfg).ConfigureAwait(false);

                if (!this.Verb.NoPublisherUpdate)
                {
                    await this.Console.WriteInfo(Resources.Localization.Signing_DeviceGuard_Reading).ConfigureAwait(false);
                    cfg.Subject = await this.DeviceGuardHelper.GetSubjectFromDeviceGuardSigning(json).ConfigureAwait(false);
                    await this.Console.WriteSuccess(string.Format(Resources.Localization.CLI_Executor_Sign_NewName_Format, cfg.Subject)).ConfigureAwait(false);
                }
            }
            catch (SdkException e)
            {
                Logger.Error().WriteLine(e);
                await this.Console.WriteError(string.Format(Resources.Localization.CLI_Executor_Sign_Error_Code_Format, "0x" + e.ExitCode.ToString("X"), e.Message));
                return e.ExitCode;
            }
            catch (Exception e)
            {
                Logger.Error().WriteLine(e);
                await this.Console.WriteError(string.Format(Resources.Localization.Signing_Failed, e.Message));
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
                    await this.Console.WriteInfo(string.Format(Resources.Localization.CLI_Executor_Sign_SigningCerFile_Format, path, fileName));
                    await this._signingManager.SignPackageWithPfx(path, updatePublisherName, pfxPath, secPass, timestamp, this.Verb.IncreaseVersion).ConfigureAwait(false);
                    await this.Console.WriteSuccess(Resources.Localization.CLI_Executor_Sign_Success).ConfigureAwait(false);
                    await this.Console.ShowCertSummary(this._signingManager, path);
                }

                return StandardExitCodes.ErrorSuccess;
            }
            catch (Exception e)
            {
                Logger.Error().WriteLine(e);
                await this.Console.WriteError(string.Format(Resources.Localization.CLI_Executor_Sign_Error_Message_Format, e.Message));
                return StandardExitCodes.ErrorGeneric;
            }
        }

        private async Task<int> SignStore(string thumbprint, string timestamp, bool updatePublisherName)
        {
            var mode = this.Verb.UseMachineStore ? CertificateStoreType.Machine : CertificateStoreType.User;
            var certificates = await this._signingManager.GetCertificatesFromStore(mode, false).ConfigureAwait(false);

            var certificate = certificates.FirstOrDefault(c => string.Equals(c.Thumbprint, thumbprint, StringComparison.Ordinal));
            if (certificate == null)
            {
                Logger.Error().WriteLine(Resources.Localization.CLI_Executor_Sign_Error_WrongThumbprint_Format, thumbprint);
                await this.Console.WriteError(string.Format(Resources.Localization.CLI_Executor_Sign_Error_WrongThumbprint_Format, thumbprint));
                return StandardExitCodes.ErrorGeneric;
            }

            try
            {
                foreach (var path in this.Verb.FilePath)
                {
                    await this.Console.WriteInfo(string.Format(Resources.Localization.CLI_Executor_Sign_SigningSha_Format, path, thumbprint));
                    await this._signingManager.SignPackageWithInstalled(path, updatePublisherName, certificate, timestamp, this.Verb.IncreaseVersion).ConfigureAwait(false);
                    await this.Console.WriteSuccess(Resources.Localization.CLI_Executor_Sign_Success).ConfigureAwait(false);
                    await this.Console.ShowCertSummary(this._signingManager, path);
                }

                return StandardExitCodes.ErrorSuccess;
            }
            catch (SdkException e)
            {
                Logger.Error().WriteLine(e);
                await this.Console.WriteError(string.Format(Resources.Localization.CLI_Executor_Sign_Error_Code_Format, "0x" + e.ExitCode.ToString("X"), e.Message));
                return e.ExitCode;
            }
            catch (Exception e)
            {
                Logger.Error().WriteLine(e);
                await this.Console.WriteError(string.Format(Resources.Localization.CLI_Executor_Sign_Error_Message_Format, e.Message));
                return StandardExitCodes.ErrorGeneric;
            }
        }
    }
}