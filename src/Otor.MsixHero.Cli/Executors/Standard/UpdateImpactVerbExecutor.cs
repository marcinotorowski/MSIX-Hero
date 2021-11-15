using System;
using System.IO;
using System.Threading.Tasks;
using Otor.MsixHero.Appx.Updates;
using Otor.MsixHero.Appx.Updates.Entities;
using Otor.MsixHero.Cli.Verbs;
using Otor.MsixHero.Infrastructure.Logging;

namespace Otor.MsixHero.Cli.Executors.Standard
{
    public class UpdateImpactVerbExecutor : VerbExecutor<UpdateImpactVerb>
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(UpdateImpactVerbExecutor));
        
        public UpdateImpactVerbExecutor(UpdateImpactVerb verb, IConsole console) : base(verb, console)
        {
        }

        public override async Task<int> Execute()
        {
            var assertion = await this.AssertInputs().ConfigureAwait(false);
            if (assertion != StandardExitCodes.ErrorSuccess)
            {
                return assertion;
            }

            await this.Console.WriteInfo("Comparing packages...").ConfigureAwait(false);
            await this.Console.WriteInfo($" * {this.Verb.OldPackagePath}").ConfigureAwait(false);
            await this.Console.WriteInfo($" * {this.Verb.NewPackagePath}").ConfigureAwait(false);

            UpdateImpactResults results;
            
            try
            {
                var updateImpactAnalyzer = new AppxUpdateImpactAnalyzer();
                results = await updateImpactAnalyzer.Analyze(this.Verb.OldPackagePath, this.Verb.NewPackagePath, this.Verb.IgnoreVersionMismatch).ConfigureAwait(false);
            }
            catch (UpdateImpactException e)
            {
                Logger.Error(e);
                switch (e.ErrorType)
                {
                    case UpgradeImpactError.Unknown:
                        await this.Console.WriteError("The packages could not be compared.").ConfigureAwait(false);
                        await this.Console.WriteError(e.Message).ConfigureAwait(false);
                        return StandardExitCodes.ErrorGeneric;

                    case UpgradeImpactError.WrongPackageFormat:
                        await this.Console.WriteError("Invalid package format.").ConfigureAwait(false);
                        await this.Console.WriteError(e.Message).ConfigureAwait(false);
                        return StandardExitCodes.ErrorFormat;

                    case UpgradeImpactError.WrongFamilyName:
                        await this.Console.WriteError("Selected packages could not be analyzed, because they are not upgradable. For an upgrade to work, the package family name must be the same.").ConfigureAwait(false);
                        await this.Console.WriteError(e.Message).ConfigureAwait(false);
                        return StandardExitCodes.ErrorFormat;

                    case UpgradeImpactError.WrongPackageVersion:
                        await this.Console.WriteError("Selected packages could not be analyzed, because the version of the 'oldPackage' is newer than the version of the 'newPackage'.").ConfigureAwait(false);
                        await this.Console.WriteWarning("Consider using command line switch --force to ignore this check.").ConfigureAwait(false);
                        return StandardExitCodes.ErrorFormat;
                }

                Logger.Error(e);
                await this.Console.WriteError(e.Message).ConfigureAwait(false);
                return StandardExitCodes.ErrorGeneric;
            }
            catch (Exception e)
            {
                Logger.Error(e);
                await this.Console.WriteError(e.Message).ConfigureAwait(false);
                return StandardExitCodes.ErrorGeneric;
            }

            await this.Console.WriteSuccess("Selected packages have been successfully compared.").ConfigureAwait(false);

            await this.Console.WriteInfo("Package size").ConfigureAwait(false);
            await this.Console.WriteInfo($" * Old = {results.OldPackageLayout.FileSize} bytes").ConfigureAwait(false);
            await this.Console.WriteInfo($" * New = {results.NewPackageLayout.FileSize} bytes").ConfigureAwait(false);
            await this.Console.WriteInfo("---------------------------------");
            await this.Console.WriteInfo("Deleted files").ConfigureAwait(false);
            await this.Console.WriteInfo($" * Count = {results.DeletedFiles.FileCount}").ConfigureAwait(false);
            await this.Console.WriteInfo($" * Size = {results.DeletedFiles.FileSize} bytes").ConfigureAwait(false);
            await this.Console.WriteInfo($" * Blocks = {results.DeletedFiles.BlockCount}").ConfigureAwait(false);
            await this.Console.WriteInfo("---------------------------------");
            await this.Console.WriteInfo("Added files").ConfigureAwait(false);
            await this.Console.WriteInfo($" * Count = {results.AddedFiles.FileCount}").ConfigureAwait(false);
            await this.Console.WriteInfo($" * Size = {results.AddedFiles.FileSize} bytes").ConfigureAwait(false);
            await this.Console.WriteInfo($" * Blocks = {results.AddedFiles.BlockCount}").ConfigureAwait(false);
            await this.Console.WriteInfo("---------------------------------");
            await this.Console.WriteInfo("Changed files").ConfigureAwait(false);
            await this.Console.WriteInfo($" * Count = {results.ChangedFiles.FileCount}").ConfigureAwait(false);
            await this.Console.WriteInfo($" * Size = {results.ChangedFiles.OldPackageFileSize} bytes -> {results.ChangedFiles.NewPackageFileSize} bytes").ConfigureAwait(false);
            await this.Console.WriteInfo($" * Blocks = {results.ChangedFiles.OldPackageBlockCount} -> {results.ChangedFiles.NewPackageBlockCount}").ConfigureAwait(false);
            await this.Console.WriteInfo("---------------------------------");
            await this.Console.WriteInfo("Unchanged files").ConfigureAwait(false);
            await this.Console.WriteInfo($" * Count = {results.UnchangedFiles.FileCount}").ConfigureAwait(false);
            await this.Console.WriteInfo($" * Size = {results.UnchangedFiles.FileSize} bytes").ConfigureAwait(false);
            await this.Console.WriteInfo($" * Blocks = {results.UnchangedFiles.BlockCount}").ConfigureAwait(false);
            await this.Console.WriteInfo("---------------------------------");
            await this.Console.WriteInfo("Update impact").ConfigureAwait(false);
            await this.Console.WriteInfo($" * Net size difference = {results.SizeDifference} bytes").ConfigureAwait(false);
            await this.Console.WriteInfo($" * Required download = {results.UpdateImpact} bytes").ConfigureAwait(false);
            await this.Console.WriteInfo("---------------------------------");

            if (string.IsNullOrEmpty(this.Verb.OutputXml))
            {
                await this.Console.WriteInfo("To get more details, rerun this command with --xml \"<output_file_path>\" switch.").ConfigureAwait(false);
            }
            else
            {
                var fileInfo = new FileInfo(Path.Combine(Environment.CurrentDirectory, this.Verb.OutputXml));
                try
                {
                    results.Export(fileInfo);
                }
                catch (ArgumentException e)
                {
                    Logger.Error(e);
                    await this.Console.WriteError("The path to XML file is invalid.");
                    return StandardExitCodes.ErrorParameter;
                }
                catch (UnauthorizedAccessException e)
                {
                    Logger.Error(e);
                    await this.Console.WriteError($"Could not write to {fileInfo.FullName}. Access denied (error code: 0x{e.HResult:X2}).");
                    return e.HResult;
                }
                catch (IOException e)
                {
                    Logger.Error(e);
                    await this.Console.WriteError($"{e.Message} (error code: 0x{e.HResult:X2})");
                    return e.HResult;
                }
                catch (Exception e)
                {
                    Logger.Error(e);
                    await this.Console.WriteError($"Could not write the results to {fileInfo.FullName} (error code: 0x{e.HResult:X2})");
                    return e.HResult;
                }

                await this.Console.WriteSuccess($"XML file with results has been saved to {fileInfo.FullName}");
            }
            
            return StandardExitCodes.ErrorSuccess;
        }

        private async Task<int> AssertInputs()
        {
            if (string.IsNullOrEmpty(this.Verb.OldPackagePath))
            { 
                await this.Console.WriteError("The old package path is required.").ConfigureAwait(false);
                return 10;
            }

            if (!File.Exists(this.Verb.OldPackagePath))
            {
                await this.Console.WriteError($"File path {this.Verb.OldPackagePath} does not exist.").ConfigureAwait(false);
                return 10;
            }
            
            if (string.IsNullOrEmpty(this.Verb.NewPackagePath))
            {
                await this.Console.WriteError("The new package path is required.").ConfigureAwait(false);
                return 10;
            }

            if (!File.Exists(this.Verb.NewPackagePath))
            {
                await this.Console.WriteError($"File path {this.Verb.NewPackagePath} does not exist.").ConfigureAwait(false);
                return 10;
            }

            if (!string.IsNullOrEmpty(this.Verb.OutputXml))
            {
                if (this.Verb.OutputXml.IndexOfAny(Path.GetInvalidPathChars()) != -1)
                {
                    await this.Console.WriteError("The path to XML file is invalid. It must be a valid Windows path.");
                    return 10;
                }
                
                if (!Uri.TryCreate(this.Verb.OutputXml, UriKind.RelativeOrAbsolute, out _))
                {
                    await this.Console.WriteError("The path to XML file is invalid.");
                    return 10;
                }
            }

            return 0;
        }
    }
}
