using System;
using System.Threading.Tasks;
using Otor.MsixHero.Appx.Signing;
using Otor.MsixHero.Cli.Helpers;
using Otor.MsixHero.Cli.Verbs;
using Otor.MsixHero.Infrastructure.Logging;
using Otor.MsixHero.Infrastructure.ThirdParty.Exceptions;

namespace Otor.MsixHero.Cli.Executors
{
    public class ExtractCertVerbExecutor : VerbExecutor<ExtractCertVerb>
    {
        private readonly ISigningManager signingManager;
        private static readonly ILog Logger = LogManager.GetLogger(typeof(ExtractCertVerbExecutor));

        public ExtractCertVerbExecutor(ExtractCertVerb verb, ISigningManager signingManager, IConsole console) : base(verb, console)
        {
            this.signingManager = signingManager;
        }

        public override async Task<int> Execute()
        {
            Logger.Info($"Extracting certificate from [{this.Verb.File}]...");

            try
            {
                await this.signingManager.ExtractCertificateFromMsix(this.Verb.File, this.Verb.Output).ConfigureAwait(false);
                await this.Console.WriteSuccess("Certificate has been extracted.");
                await this.Console.ShowCertSummary(signingManager, this.Verb.File);
                return 0;
            }
            catch (SdkException e)
            {
                Logger.Error(e);
                await this.Console.WriteError(e.Message);
                return e.ExitCode;
            }
            catch (Exception e)
            {
                Logger.Error(e);
                await this.Console.WriteError(e.Message);
                return 1;
            }
        }
    }
}