using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Otor.MsixHero.Cli.Verbs.Edit.Bulk;

namespace Otor.MsixHero.Cli.Executors.Edit.Bulk
{
    public class ListEditVerbExecutor : BaseEditVerbExecutor<ListEditVerb>
    {
        private readonly IList<string> commands = new List<string>();

        public ListEditVerbExecutor(string package, ListEditVerb verb, IConsole console) : base(package, verb, console)
        {
        }

        protected override async Task OnBegin()
        {
            await base.OnBegin().ConfigureAwait(false);
            
            if (this.Verb.File == null)
            {
                await this.Console.WriteInfo(Resources.Localization.CLI_Executor_List_NoFileDefined);
                await this.Console.WriteInfo(Resources.Localization.CLI_Executor_List_NoFileDefined_Instructions).ConfigureAwait(false);

                while (true)
                {
                    var f = System.Console.ForegroundColor;
                    var b = System.Console.BackgroundColor;
                    try
                    {
                        System.Console.ForegroundColor = ConsoleColor.White;
                        System.Console.ForegroundColor = ConsoleColor.Blue;
                        System.Console.Write(@"MSIX Hero CMD");
                    }
                    finally
                    {
                        System.Console.BackgroundColor = b;
                        System.Console.ForegroundColor = f;
                    }

                    System.Console.Write(@" > ");

                    var input = System.Console.ReadLine();
                    if (string.IsNullOrEmpty(input))
                    {
                        break;
                    }

                    if (!input.TrimStart().StartsWith("#", StringComparison.OrdinalIgnoreCase))
                    {
                        this.commands.Add(input);
                    }
                }
            }
            else if (File.Exists(this.Verb.File))
            {
                foreach (var line in await File.ReadAllLinesAsync(this.Verb.File))
                {
                    if (string.IsNullOrEmpty(line) || line.TrimStart().StartsWith("#", StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }

                    this.commands.Add(line);
                }
            }
        }

        protected override async Task<int> Validate()
        {
            var baseValidation = await base.Validate().ConfigureAwait(false);
            if (baseValidation != StandardExitCodes.ErrorSuccess)
            {
                return baseValidation;
            }

            if (this.Verb.File == null)
            {
                if (!this.commands.Any())
                {
                    await this.Console.WriteError(Resources.Localization.CLI_Executor_List_Error_NoCommands).ConfigureAwait(false);
                    return StandardExitCodes.ErrorGeneric;
                }
            }
            else if (File.Exists(this.Verb.File))
            {
                if (!this.commands.Any())
                {
                    await this.Console.WriteError(Resources.Localization.CLI_Executor_List_Error_NoCommandsInFile).ConfigureAwait(false);
                }
            }
            else
            {
                await this.Console.WriteError(string.Format(Resources.Localization.CLI_Executor_List_Error_MissingFile_Format, this.Verb.File)).ConfigureAwait(false);
                return StandardExitCodes.ErrorGeneric;
            }

            var factory = new VerbExecutorFactory(new ConsoleImpl(System.Console.Out, System.Console.Error));
            foreach (var line in this.commands)
            {
                var parsedArgs = NativeMethods.ConvertArgumentsStringToArray(line.Trim());

                try
                {
                    factory.ThrowIfInvalidArguments(parsedArgs, true);
                }
                catch (VerbExecutorFactory.ArgumentsParsingException ape)
                {
                    await this.Console.WriteError(string.Format(Resources.Localization.CLI_Executor_List_Error_Parsing_Format, line)).ConfigureAwait(false);
                    foreach (var err in ape.Errors)
                    {
                        await this.Console.WriteError("  * " + err).ConfigureAwait(false);
                    }

                    return StandardExitCodes.ErrorParameter;
                }
            }

            return StandardExitCodes.ErrorSuccess;
        }

        protected override async Task<int> ExecuteOnExtractedPackage(string directoryPath)
        {
            foreach (var line in this.commands)
            {
                try
                {
                    var parsedArgs = NativeMethods.ConvertArgumentsStringToArray(line.Trim());

                    var factory = new VerbExecutorFactory(new ConsoleImpl(System.Console.Out, System.Console.Error));
                    var executor = factory.CreateEditVerbExecutor(directoryPath, parsedArgs, true);
                    if (executor == null)
                    {
                        return StandardExitCodes.ErrorGeneric;
                    }

                    Environment.ExitCode = await executor.Execute().ConfigureAwait(false);
                    if (Environment.ExitCode != StandardExitCodes.ErrorSuccess)
                    {
                        return Environment.ExitCode;
                    }
                }
                catch (VerbExecutorFactory.ArgumentsParsingException ape)
                {
                    await this.Console.WriteError(string.Format(Resources.Localization.CLI_Executor_List_Error_Parsing_Format, line)).ConfigureAwait(false);
                    foreach (var err in ape.Errors)
                    {
                        await this.Console.WriteError("  * " + err).ConfigureAwait(false);
                    }

                    return StandardExitCodes.ErrorParameter;
                }
                catch (Exception)
                {
                    await this.Console.WriteError(string.Format(Resources.Localization.CLI_Executor_List_Error_Parsing_Format, line)).ConfigureAwait(false);
                    return StandardExitCodes.ErrorGeneric;
                }
            }
            return StandardExitCodes.ErrorSuccess;
        }
    }
}
