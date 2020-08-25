using System;
using System.Threading.Tasks;
using Otor.MsixHero.Cli.Verbs;
using Otor.MsixHero.Infrastructure.Logging;
using Otor.MsixHero.Infrastructure.ThirdParty.Exceptions;
using Otor.MsixHero.Infrastructure.ThirdParty.Sdk;

namespace Otor.MsixHero.Cli.Executors
{
    public class UnpackVerbExecutor : VerbExecutor<UnpackVerb>
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(UnpackVerbExecutor));

        public UnpackVerbExecutor(UnpackVerb verb, IConsole console) : base(verb, console)
        {
        }

        public override async Task<int> Execute()
        {
            var msixSdkWrapper = new MsixSdkWrapper();

            Logger.Info($"Unpacking [{this.Verb.Package}] to [{this.Verb.Directory}]...");

            try
            {
                await this.Console.WriteInfo($"Unpacking [{this.Verb.Directory}] to [{this.Verb.Package}]...").ConfigureAwait(false);
                await msixSdkWrapper.UnpackPackage(this.Verb.Package, this.Verb.Directory).ConfigureAwait(false);

                await this.Console.WriteSuccess($"Package has been unpacked to [{this.Verb.Directory}].");
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
