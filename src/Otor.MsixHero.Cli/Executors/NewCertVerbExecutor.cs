using System;
using System.IO;
using System.Threading.Tasks;
using Otor.MsixHero.Appx.Signing;
using Otor.MsixHero.Cli.Verbs;
using Otor.MsixHero.Infrastructure.Logging;

namespace Otor.MsixHero.Cli.Executors
{
    public class NewCertVerbExecutor : VerbExecutor<NewCertVerb>
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(NewCertVerbExecutor));

        private readonly ISigningManager signingManager;

        public NewCertVerbExecutor(NewCertVerb verb, ISigningManager signingManager, IConsole console) : base(verb, console)
        {
            this.signingManager = signingManager;
        }

        public override async Task<int> Execute()
        {
            var output = new DirectoryInfo(this.Verb.OutputFolder);
            var subject = this.Verb.Subject;
            if (subject == null)
            {
                subject = "CN=" + this.Verb.DisplayName;
            }

            await this.Console.WriteInfo($"Creating a new certificate for [{this.Verb.DisplayName}]...").ConfigureAwait(false);

            try
            {
                if (!output.Exists)
                {
                    output.Create();
                }

                var pfxFile = await signingManager.CreateSelfSignedCertificate(output, subject, this.Verb.DisplayName, this.Verb.Password, this.Verb.ValidUntil ?? DateTime.Now.AddDays(365)).ConfigureAwait(false);
                await this.Console.WriteSuccess($"Certificate has been saved to {pfxFile}.").ConfigureAwait(false);

                if (this.Verb.Import)
                {
                    await this.Console.WriteInfo("Importing certificate file to the list of trusted people...").ConfigureAwait(false);
                    await this.signingManager.Trust(Path.ChangeExtension(pfxFile, ".cer")).ConfigureAwait(false);
                    await this.Console.WriteSuccess($"Certificate has been imported to the Trusted People store.").ConfigureAwait(false);
                }

                return 0;
            }
            catch (Exception e)
            {
                Logger.Error(e);
                await this.Console.WriteError(e.Message).ConfigureAwait(false);
                return 10;
            }
        }
    }
}
