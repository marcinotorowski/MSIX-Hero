using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CommandLine;
using Otor.MsixHero.Appx.Packaging.ModificationPackages;
using Otor.MsixHero.Appx.Packaging.Packer;
using Otor.MsixHero.Appx.Signing;
using Otor.MsixHero.Appx.WindowsVirtualDesktop.AppAttach;
using Otor.MsixHero.Cli.Executors;
using Otor.MsixHero.Cli.Verbs;
using Otor.MsixHero.Infrastructure.Services;

namespace Otor.MsixHero.Cli
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var p = Parser.Default.ParseArguments<SignVerb, PackVerb, UnpackVerb, NewCertVerb, TrustVerb, AppAttachVerb, NewModPackVerb, ExtractCertVerb>(args);
            await p.WithParsedAsync<SignVerb>(Run);
            await p.WithParsedAsync<PackVerb>(Run);
            await p.WithParsedAsync<UnpackVerb>(Run);
            await p.WithParsedAsync<NewCertVerb>(Run);
            await p.WithParsedAsync<NewModPackVerb>(Run);
            await p.WithParsedAsync<TrustVerb>(Run);
            await p.WithParsedAsync<ExtractCertVerb>(Run);
            await p.WithParsedAsync<AppAttachVerb>(Run);
            await p.WithNotParsedAsync(Run);
        }

        private static Task Run(IEnumerable<Error> arg)
        {
            var err = arg.FirstOrDefault();
            if (err != null)
            {
                Environment.ExitCode = (int) err.Tag;
            }

            return Task.FromResult(1);
        }

        private static async Task<int> Run(PackVerb arg)
        {
            var console = new ConsoleImpl(Console.Out, Console.Error);
            var executor = new PackVerbExecutor(arg, console);
            var exitCode = await executor.Execute().ConfigureAwait(false);
            Environment.ExitCode = exitCode;
            return exitCode;
        }

        private static async Task<int> Run(AppAttachVerb arg)
        {
            var console = new ConsoleImpl(Console.Out, Console.Error);
            IAppAttachManager appAttachManager = new AppAttachManager(new SigningManager());
            var executor = new AppAttachVerbExecutor(arg, appAttachManager, console);
            var exitCode = await executor.Execute().ConfigureAwait(false);
            Environment.ExitCode = exitCode;
            return exitCode;
        }

        private static async Task<int> Run(TrustVerb arg)
        {
            var console = new ConsoleImpl(Console.Out, Console.Error);
            
            var signingManager = new SigningManager();
            var executor = new TrustVerbExecutor(arg, signingManager, console);
            var exitCode = await executor.Execute().ConfigureAwait(false);
            Environment.ExitCode = exitCode;
            return exitCode;
        }

        private static async Task<int> Run(ExtractCertVerb arg)
        {
            var console = new ConsoleImpl(Console.Out, Console.Error);
            
            var signingManager = new SigningManager();
            var executor = new ExtractCertVerbExecutor(arg, signingManager, console);
            var exitCode = await executor.Execute().ConfigureAwait(false);
            Environment.ExitCode = exitCode;
            return exitCode;
        }

        private static async Task<int> Run(NewModPackVerb arg)
        {
            var console = new ConsoleImpl(Console.Out, Console.Error);

            var executor = new NewModPackVerbExecutor(arg, new AppxContentBuilder(new AppxPacker()), console);
            var exitCode = await executor.Execute().ConfigureAwait(false);
            Environment.ExitCode = exitCode;
            return exitCode;
        }

        private static async Task<int> Run(UnpackVerb arg)
        {
            var console = new ConsoleImpl(Console.Out, Console.Error);
            var executor = new UnpackVerbExecutor(arg, console);
            var exitCode = await executor.Execute().ConfigureAwait(false);
            Environment.ExitCode = exitCode;
            return exitCode;
        }

        private static async Task<int> Run(NewCertVerb arg)
        {
            var signingManager = new SigningManager();
            var console = new ConsoleImpl(Console.Out, Console.Error);
            var executor = new NewCertVerbExecutor(arg, signingManager, console);
            var exitCode = await executor.Execute().ConfigureAwait(false);
            Environment.ExitCode = exitCode;
            return exitCode;
        }

        private static async Task<int> Run(SignVerb arg)
        {
            var signingManager = new SigningManager();
            var configurationService = new LocalConfigurationService();

            var console = new ConsoleImpl(Console.Out, Console.Error);
            var executor = new SignVerbExecutor(arg, signingManager, configurationService, console);
            var exitCode = await executor.Execute().ConfigureAwait(false);
            Environment.ExitCode = exitCode;
            return exitCode;
        }
    }
}
