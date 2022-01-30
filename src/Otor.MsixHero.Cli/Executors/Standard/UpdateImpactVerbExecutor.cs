using System;
using System.IO;
using System.Threading.Tasks;
using Otor.MsixHero.Appx.Updates;
using Otor.MsixHero.Appx.Updates.Entities;
using Otor.MsixHero.Cli.Verbs;
using Dapplo.Log;

namespace Otor.MsixHero.Cli.Executors.Standard
{
    public class UpdateImpactVerbExecutor : VerbExecutor<UpdateImpactVerb>
    {
        private static readonly LogSource Logger = new();        
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

            await this.Console.WriteInfo(Resources.Localization.CLI_Executor_UpdateImpact_Comparing).ConfigureAwait(false);
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
                Logger.Error().WriteLine(e);
                switch (e.ErrorType)
                {
                    case UpgradeImpactError.Unknown:
                        await this.Console.WriteError(Resources.Localization.CLI_Executor_UpdateImpact_Error_Unknown).ConfigureAwait(false);
                        await this.Console.WriteError(e.Message).ConfigureAwait(false);
                        return StandardExitCodes.ErrorGeneric;

                    case UpgradeImpactError.WrongPackageFormat:
                        await this.Console.WriteError(Resources.Localization.CLI_Executor_UpdateImpact_Error_Format).ConfigureAwait(false);
                        await this.Console.WriteError(e.Message).ConfigureAwait(false);
                        return StandardExitCodes.ErrorFormat;

                    case UpgradeImpactError.WrongFamilyName:
                        await this.Console.WriteError(Resources.Localization.CLI_Executor_UpdateImpact_Error_NotSameFamily).ConfigureAwait(false);
                        await this.Console.WriteError(e.Message).ConfigureAwait(false);
                        return StandardExitCodes.ErrorFormat;

                    case UpgradeImpactError.WrongPackageVersion:
                        await this.Console.WriteError(Resources.Localization.CLI_Executor_UpdateImpact_Error_Versioning).ConfigureAwait(false);
                        await this.Console.WriteWarning(Resources.Localization.CLI_Executor_UpdateImpact_Error_Versioning_Hint).ConfigureAwait(false);
                        return StandardExitCodes.ErrorFormat;
                }

                Logger.Error().WriteLine(e);
                await this.Console.WriteError(e.Message).ConfigureAwait(false);
                return StandardExitCodes.ErrorGeneric;
            }
            catch (Exception e)
            {
                Logger.Error().WriteLine(e);
                await this.Console.WriteError(e.Message).ConfigureAwait(false);
                return StandardExitCodes.ErrorGeneric;
            }

            await this.Console.WriteSuccess(Resources.Localization.CLI_Executor_UpdateImpact_Success).ConfigureAwait(false);

            await this.Console.WriteInfo(Resources.Localization.CLI_Executor_UpdateImpact_Summary_PackageSize).ConfigureAwait(false);
            await this.Console.WriteInfo(" * " + string.Format(Resources.Localization.CLI_Executor_UpdateImpact_Summary_OldBytes_Format, results.OldPackageLayout.FileSize)).ConfigureAwait(false);
            await this.Console.WriteInfo(" * " + string.Format(Resources.Localization.CLI_Executor_UpdateImpact_Summary_NewBytes_Format, results.NewPackageLayout.FileSize)).ConfigureAwait(false);
            await this.Console.WriteInfo("---------------------------------");
            await this.Console.WriteInfo(Resources.Localization.CLI_Executor_UpdateImpact_Summary_Deleted).ConfigureAwait(false);
            await this.Console.WriteInfo(" * " + string.Format(Resources.Localization.CLI_Executor_UpdateImpact_Summary_Count_Format, results.DeletedFiles.FileCount)).ConfigureAwait(false);
            await this.Console.WriteInfo(" * " + string.Format(Resources.Localization.CLI_Executor_UpdateImpact_Summary_Size_Format, results.DeletedFiles.FileSize)).ConfigureAwait(false);
            await this.Console.WriteInfo(" * " + string.Format(Resources.Localization.CLI_Executor_UpdateImpact_Summary_Blocks_Format, results.DeletedFiles.BlockCount)).ConfigureAwait(false);
            await this.Console.WriteInfo("---------------------------------");
            await this.Console.WriteInfo(Resources.Localization.CLI_Executor_UpdateImpact_Summary_Added).ConfigureAwait(false);
            await this.Console.WriteInfo(" * " + string.Format(Resources.Localization.CLI_Executor_UpdateImpact_Summary_Count_Format, results.AddedFiles.FileCount)).ConfigureAwait(false);
            await this.Console.WriteInfo(" * " + string.Format(Resources.Localization.CLI_Executor_UpdateImpact_Summary_Size_Format, results.AddedFiles.FileSize)).ConfigureAwait(false);
            await this.Console.WriteInfo(" * " + string.Format(Resources.Localization.CLI_Executor_UpdateImpact_Summary_Blocks_Format, results.AddedFiles.BlockCount)).ConfigureAwait(false);
            await this.Console.WriteInfo("---------------------------------");
            await this.Console.WriteInfo(Resources.Localization.CLI_Executor_UpdateImpact_Summary_Changed).ConfigureAwait(false);
            await this.Console.WriteInfo(" * " + string.Format(Resources.Localization.CLI_Executor_UpdateImpact_Summary_Count_Format, results.ChangedFiles.FileCount)).ConfigureAwait(false);
            await this.Console.WriteInfo(" * " + string.Format(Resources.Localization.CLI_Executor_UpdateImpact_Summary_SizeChanged_Format, results.ChangedFiles.OldPackageFileSize, results.ChangedFiles.NewPackageFileSize)).ConfigureAwait(false);
            await this.Console.WriteInfo(" * " + string.Format(Resources.Localization.CLI_Executor_UpdateImpact_Summary_BlocksChanged_Format, results.ChangedFiles.OldPackageBlockCount, results.ChangedFiles.NewPackageBlockCount)).ConfigureAwait(false);
            await this.Console.WriteInfo("---------------------------------");
            await this.Console.WriteInfo(Resources.Localization.CLI_Executor_UpdateImpact_Summary_Unchanged).ConfigureAwait(false);
            await this.Console.WriteInfo(" * " + string.Format(Resources.Localization.CLI_Executor_UpdateImpact_Summary_Count_Format, results.UnchangedFiles.FileCount)).ConfigureAwait(false);
            await this.Console.WriteInfo(" * " + string.Format(Resources.Localization.CLI_Executor_UpdateImpact_Summary_Size_Format, results.UnchangedFiles.FileSize)).ConfigureAwait(false);
            await this.Console.WriteInfo(" * " + string.Format(Resources.Localization.CLI_Executor_UpdateImpact_Summary_Blocks_Format, results.UnchangedFiles.BlockCount)).ConfigureAwait(false);
            await this.Console.WriteInfo("---------------------------------");
            await this.Console.WriteInfo(Resources.Localization.CLI_Executor_UpdateImpact_Summary_UpdateImpact).ConfigureAwait(false);
            await this.Console.WriteInfo($" * " + string.Format(Resources.Localization.CLI_Executor_UpdateImpact_Summary_NetSizeDiff_Format, results.SizeDifference)).ConfigureAwait(false);
            await this.Console.WriteInfo($" * " + string.Format(Resources.Localization.CLI_Executor_UpdateImpact_Summary_RequiredDownload_Format, results.UpdateImpact)).ConfigureAwait(false);
            await this.Console.WriteInfo("---------------------------------");

            if (string.IsNullOrEmpty(this.Verb.OutputXml))
            {
                await this.Console.WriteInfo(Resources.Localization.CLI_Executor_UpdateImpact_Success_MoreDetails).ConfigureAwait(false);
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
                    Logger.Error().WriteLine(e);
                    await this.Console.WriteError(Resources.Localization.CLI_Executor_UpdateImpact_Error_InvalidXMLPath);
                    return StandardExitCodes.ErrorParameter;
                }
                catch (UnauthorizedAccessException e)
                {
                    Logger.Error().WriteLine(e);
                    await this.Console.WriteError(string.Format(Resources.Localization.CLI_Executor_UpdateImpact_Error_AccessDenied_Format, fileInfo.FullName, "0x" + e.HResult.ToString("X2")));
                    return e.HResult;
                }
                catch (IOException e)
                {
                    Logger.Error().WriteLine(e);
                    await this.Console.WriteError(string.Format(Resources.Localization.CLI_Executor_UpdateImpact_Error_MessageCode_Format, e.Message, "0x" + e.HResult.ToString("X2")));
                    return e.HResult;
                }
                catch (Exception e)
                {
                    Logger.Error().WriteLine(e);
                    await this.Console.WriteError(string.Format(Resources.Localization.CLI_Executor_UpdateImpact_Error_UnknownCode_Format, fileInfo.FullName, "0x" + e.HResult.ToString("X2")));
                    return e.HResult;
                }

                await this.Console.WriteSuccess(string.Format(Resources.Localization.CLI_Executor_UpdateImpact_Success_File_Format, fileInfo.FullName));
            }
            
            return StandardExitCodes.ErrorSuccess;
        }

        private async Task<int> AssertInputs()
        {
            if (string.IsNullOrEmpty(this.Verb.OldPackagePath))
            { 
                await this.Console.WriteError(Resources.Localization.CLI_Executor_UpdateImpact_Error_OldPackageMissing).ConfigureAwait(false);
                return 10;
            }

            if (!File.Exists(this.Verb.OldPackagePath))
            {
                await this.Console.WriteError(string.Format(Resources.Localization.CLI_Error_FileNotExists_Format, this.Verb.OldPackagePath)).ConfigureAwait(false);
                return 10;
            }
            
            if (string.IsNullOrEmpty(this.Verb.NewPackagePath))
            {
                await this.Console.WriteError(Resources.Localization.CLI_Executor_UpdateImpact_Error_NewPackageMissing).ConfigureAwait(false);
                return 10;
            }

            if (!File.Exists(this.Verb.NewPackagePath))
            {
                await this.Console.WriteError(string.Format(Resources.Localization.CLI_Error_FileNotExists_Format, this.Verb.NewPackagePath)).ConfigureAwait(false);
                return 10;
            }

            if (!string.IsNullOrEmpty(this.Verb.OutputXml))
            {
                if (this.Verb.OutputXml.IndexOfAny(Path.GetInvalidPathChars()) != -1)
                {
                    await this.Console.WriteError(Resources.Localization.CLI_Executor_UpdateImpact_Error_InvalidWindowsPath);
                    return 10;
                }
                
                if (!Uri.TryCreate(this.Verb.OutputXml, UriKind.RelativeOrAbsolute, out _))
                {
                    await this.Console.WriteError(Resources.Localization.CLI_Executor_UpdateImpact_Error_InvalidPath);
                    return 10;
                }
            }

            return 0;
        }
    }
}
