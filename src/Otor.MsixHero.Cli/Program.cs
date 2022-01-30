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
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using CommandLine.Text;
using Otor.MsixHero.Cli.Executors;
using Otor.MsixHero.Infrastructure.Helpers;
using Otor.MsixHero.Infrastructure.Localization;
using Otor.MsixHero.Infrastructure.Logging;
using Otor.MsixHero.Infrastructure.Services;

namespace Otor.MsixHero.Cli
{
    public class Program
    {
        static async Task<int> Main(string[] args)
        {
            var logLevel = MsixHeroLogLevel.Trace;
            
            ExceptionGuard.Guard(() =>
            {
                var service = new LocalConfigurationService();
                var config = service.GetCurrentConfiguration();

                var language = config.UiConfiguration?.Language;
                if (!string.IsNullOrEmpty(language))
                {
                    ExceptionGuard.Guard(() => MsixHeroTranslation.Instance.ChangeCulture(CultureInfo.GetCultureInfo(language)));
                }

                System.Threading.Thread.CurrentThread.CurrentCulture = MsixHeroTranslation.Instance.CurrentCulture;
                System.Threading.Thread.CurrentThread.CurrentUICulture = MsixHeroTranslation.Instance.CurrentCulture;
                logLevel = config.VerboseLogging ? MsixHeroLogLevel.Trace : MsixHeroLogLevel.Info;
            });

            LogManager.Initialize(logLevel);
            SentenceBuilder.Factory = () => new LocalizableSentenceBuilder();

            var console = new ConsoleImpl(Console.Out, Console.Error);
            try
            {
                // First argument is the "edit" verb.
                // Second argument is the path of the package.
                if (args.Length > 1 && 
                    string.Equals(args[0], "edit", StringComparison.OrdinalIgnoreCase) && 
                    !args[1].StartsWith("-", StringComparison.OrdinalIgnoreCase))
                {
                    return await DoEditVerb(args[1], args.Skip(2), console);
                }

                return await DoCommonVerbs(args, console);
            }
            catch (Exception e)
            {
                await console.WriteError(e.Message).ConfigureAwait(false);
                Environment.ExitCode = 1;
                return 1;
            }
        }

        private static async Task<int> DoCommonVerbs(IEnumerable<string> args, IConsole console)
        {
            var factory = new VerbExecutorFactory(console);
            var verbExecutor = factory.CreateStandardVerbExecutor(args);

            if (verbExecutor != null)
            {
                var exitCode = await verbExecutor.Execute().ConfigureAwait(false);
                Environment.ExitCode = exitCode;
                return exitCode;
            }

            return Environment.ExitCode;
        }

        private static async Task<int> DoEditVerb(string packagePath, IEnumerable<string> args, IConsole console)
        {
            var factory = new VerbExecutorFactory(console);
            
            var verbExecutor = factory.CreateEditVerbExecutor(packagePath, args);

            if (verbExecutor != null)
            {
                var exitCode = await verbExecutor.Execute().ConfigureAwait(false);
                Environment.ExitCode = exitCode;
                return exitCode;
            }

            return Environment.ExitCode;
        }
    }
}
