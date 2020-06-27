using System;
using System.Threading.Tasks;
using otor.msixhero.cmd.Helpers;
using otor.msixhero.cmd.Verbs;
using otor.msixhero.lib.BusinessLayer.Managers.Signing;
using otor.msixhero.lib.Infrastructure.Interop;
using otor.msixhero.lib.Infrastructure.Logging;

namespace otor.msixhero.cmd.Executors
{
    public class TrustVerbExecutor : VerbExecutor<TrustVerb>
    {
        private readonly ISigningManager signingManager;
        private static readonly ILog Logger = LogManager.GetLogger(typeof(TrustVerbExecutor));

        public TrustVerbExecutor(TrustVerb verb, ISigningManager signingManager, IConsole console) : base(verb, console)
        {
            this.signingManager = signingManager;
        }

        public override async Task<int> Execute()
        {
            Logger.Info($"Importing certificate from [{this.Verb.File}]...");

            try
            {
                await this.signingManager.Trust(this.Verb.File).ConfigureAwait(false);
                await this.Console.WriteSuccess("Certificate has been imported.");
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
