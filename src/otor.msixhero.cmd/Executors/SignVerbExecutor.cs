using System;
using System.IO;
using System.Linq;
using System.Security;
using System.Threading.Tasks;
using CommandLine;
using otor.msixhero.cmd.Helpers;
using otor.msixhero.cmd.Verbs;
using otor.msixhero.lib.BusinessLayer.Managers.Signing;
using otor.msixhero.lib.Domain.Appx.Signing;
using otor.msixhero.lib.Infrastructure.Configuration;
using otor.msixhero.lib.Infrastructure.Crypt;
using otor.msixhero.lib.Infrastructure.Interop;
using otor.msixhero.lib.Infrastructure.Logging;

namespace otor.msixhero.cmd.Executors
{
    public class SignVerbExecutor : VerbExecutor<SignVerb>
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(SignVerbExecutor));
        private readonly ISigningManager signingManager;
        private readonly IConfigurationService configurationService;

        public SignVerbExecutor(SignVerb signVerb, ISigningManager signingManager, IConfigurationService configurationService, IConsole console) : base(signVerb, console)
        {
            this.signingManager = signingManager;
            this.configurationService = configurationService;
        }

        public override async Task<int> Execute()
        {
            var config = await configurationService.GetCurrentConfigurationAsync().ConfigureAwait(false);

            if (this.Verb.ThumbPrint != null)
            {
                if (this.Verb.PfxFilePath != null)
                {
                    await this.Console.WriteError("Certificate thumbprint and PFX file path cannot be used together.").ConfigureAwait(false);
                    Logger.Error("Certificate thumbprint and PFX file path cannot be used together.");
                    return (int) ErrorType.MissingValueOptionError;
                }

                return await this.SignStore(this.Verb.ThumbPrint, this.Verb.TimeStampUrl ?? config.Signing?.TimeStampServer).ConfigureAwait(false);
            }

            if (this.Verb.PfxFilePath != null)
            {
                return await this.SignPfx(this.Verb.PfxFilePath, this.Verb.PfxPassword, this.Verb.TimeStampUrl ?? config.Signing?.TimeStampServer).ConfigureAwait(false);
            }
            
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
                            return 10;
                        }
                    }
                        
                    return await this.SignPfx(config.Signing.PfxPath.Resolved, password, this.Verb.TimeStampUrl ?? config.Signing?.TimeStampServer).ConfigureAwait(false);
                case CertificateSource.Personal:
                    return await this.SignStore(config.Signing.Thumbprint, this.Verb.TimeStampUrl ?? config.Signing?.TimeStampServer);
                default:
                    Logger.Error("No certificate has been provided, and no default certificate has been configured in MSIX Hero settings.");
                    await this.Console.WriteError("No certificate has been provided, and no default certificate has been configured in MSIX Hero settings.").ConfigureAwait(false);
                    return 10;
            }
        }

        private async Task<int> SignPfx(string pfxPath, string password, string timestamp)
        {
            try
            {
                using (var secPass = new SecureString())
                {
                    foreach (var passChar in password ?? String.Empty)
                    {
                        secPass.AppendChar(passChar);
                    }

                    var fileName = Path.GetFileName(pfxPath);
                    foreach (var path in this.Verb.FilePath)
                    {
                        await this.Console.WriteInfo($"Signing '{path}' with certificate [{fileName}]...");
                        await this.signingManager.SignPackage(path, true, pfxPath, secPass, timestamp, this.Verb.IncreaseVersion).ConfigureAwait(false);
                        await this.Console.WriteSuccess("Package signed successfully!").ConfigureAwait(false);
                        await this.Console.ShowCertSummary(signingManager, path);
                    }
                }

                return 0;
            }
            catch (Exception e)
            {
                Logger.Error(e);
                await this.Console.WriteError($"Signing failed: {e.Message}");
                return 10;
            }
        }

        private async Task<int> SignStore(string thumbprint, string timestamp)
        {
            var mode = this.Verb.UseMachineStore ? CertificateStoreType.Machine : CertificateStoreType.User;
            var certificates = await this.signingManager.GetCertificatesFromStore(mode, false).ConfigureAwait(false);

            var certificate = certificates.FirstOrDefault(c => string.Equals(c.Thumbprint, thumbprint, StringComparison.Ordinal));
            if (certificate == null)
            {
                Logger.Error("Certificate with thumbprint {0} was not found.", thumbprint);
                await this.Console.WriteError($"Certificate with thumbprint {thumbprint} could not be found");
                return 10;
            }

            try
            {
                foreach (var path in this.Verb.FilePath)
                {
                    await this.Console.WriteInfo($"Signing '{path}' with certificate [SHA1 = {thumbprint}]...");
                    await this.signingManager.SignPackage(path, true, certificate, timestamp, this.Verb.IncreaseVersion).ConfigureAwait(false);
                    await this.Console.WriteSuccess("Package signed successfully!").ConfigureAwait(false);
                    await this.Console.ShowCertSummary(signingManager, path);
                }

                return 0;
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
                return 10;
            }
        }
    }
}