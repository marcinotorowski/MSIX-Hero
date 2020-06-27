﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CommandLine;
using otor.msixhero.cmd.Executors;
using otor.msixhero.cmd.Verbs;
using otor.msixhero.lib.BusinessLayer.Managers.Signing;
using otor.msixhero.lib.Infrastructure.Configuration;

namespace otor.msixhero.cmd
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var p = Parser.Default.ParseArguments<SignVerb, PackVerb, UnpackVerb, NewCertVerb, TrustVerb>(args);
            await p.WithParsedAsync<SignVerb>(Run);
            await p.WithParsedAsync<PackVerb>(Run);
            await p.WithParsedAsync<UnpackVerb>(Run);
            await p.WithParsedAsync<NewCertVerb>(Run);
            await p.WithParsedAsync<TrustVerb>(Run);
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

        private static async Task<int> Run(TrustVerb arg)
        {
            var console = new ConsoleImpl(Console.Out, Console.Error);
            
            var signingManager = new SigningManager();
            var executor = new TrustVerbExecutor(arg, signingManager, console);
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
