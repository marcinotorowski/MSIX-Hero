using Otor.MsixHero.Appx.Packaging;
using Otor.MsixHero.Infrastructure.Helpers;
using Otor.MsixHero.Infrastructure.ThirdParty.Sdk;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using Otor.MsixHero.Appx.Editor.Facades;
using Otor.MsixHero.Appx.Editor.Helpers;
using Otor.MsixHero.Cli.Verbs.Edit;

namespace Otor.MsixHero.Cli.Executors.Edit
{
    public abstract class BaseEditVerbExecutor<T> : VerbExecutor<T> where T : BaseEditVerb
    {
        private readonly string _package;

        protected BaseEditVerbExecutor(string package, T verb, IConsole console) : base(verb, console)
        {
            this._package = package;
        }
        
        public override async Task<int> Execute()
        {
            try
            {
                await this.OnBegin().ConfigureAwait(false);

                if (!File.Exists(this._package) && !Directory.Exists(this._package))
                {
                    await this.Console.WriteError(string.Format(Resources.Localization.CLI_Executor_Error_PathNotExist_Format, this._package));
                    return 10;
                }

                var validation = await this.Validate().ConfigureAwait(false);
                if (validation != 0)
                {
                    return validation;
                }

                if (File.Exists(this._package))
                {
                    // This is a file…
                    if (string.Equals(Path.GetFileName(this._package), FileConstants.AppxManifestFile, StringComparison.OrdinalIgnoreCase))
                    {
                        // .. a manifest file
                        var result = await this.ExecuteOnExtractedPackage(Path.GetDirectoryName(this._package)).ConfigureAwait(false);
                        await this.OnFinished().ConfigureAwait(false);
                        return result;
                    }

                    if (string.Equals(".msix", Path.GetExtension(this._package)))
                    {
                        // .. an MSIX package
                        var msixMgr = new MakeAppxWrapper();
                        var tempFolder = Path.Combine(Path.GetTempPath(), "msixhero-" + Guid.NewGuid().ToString("N").Substring(0, 8));

                        try
                        {
                            await this.Console.WriteInfo(string.Format(Resources.Localization.CLI_Executor_OpeningFile_Format, Path.GetFileName(this._package))).ConfigureAwait(false);

                            // 1) Unpack first
                            await msixMgr.Unpack(MakeAppxUnpackOptions.Create(this._package, tempFolder)).ConfigureAwait(false);

                            // 2) Make edit
                            var result = await this.ExecuteOnExtractedPackage(tempFolder).ConfigureAwait(false);
                            if (result != StandardExitCodes.ErrorSuccess)
                            {
                                await this.Console.WriteWarning(Resources.Localization.CLI_Executor_Warn_SkippedDueToPreviousErrors).ConfigureAwait(false);
                                return result;
                            }

                            // 3) Add branding
                            XDocument document;
                            await using (var fs = File.OpenRead(Path.Combine(tempFolder, "AppxManifest.xml")))
                            {
                                document = await XDocument.LoadAsync(fs, LoadOptions.None, CancellationToken.None).ConfigureAwait(false);
                            }

                            var inject = new MsixHeroBrandingInjector();
                            await inject.Inject(document).ConfigureAwait(false);
                            var writer = new AppxDocumentWriter(document);
                            await writer.WriteAsync(Path.Combine(tempFolder, "AppxManifest.xml")).ConfigureAwait(false);

                            if (result == StandardExitCodes.ErrorSuccess)
                            {
                                await this.Console.WriteInfo(string.Format(Resources.Localization.CLI_Executor_SavingFile_Format, Path.GetFileName(this._package))).ConfigureAwait(false);
                                // 3) Pack again
                                await msixMgr.Pack(MakeAppxPackOptions.CreateFromDirectory(tempFolder, this._package, false)).ConfigureAwait(false);
                                await this.OnFinished().ConfigureAwait(false);
                            }

                            return result;
                        }
                        finally
                        {
                            if (Directory.Exists(tempFolder))
                            {
                                ExceptionGuard.Guard(() => Directory.Delete(tempFolder, true));
                            }
                        }
                    }
                }
                else if (Directory.Exists(this._package))
                {
                    // this is extracted directory
                    var manifestPath = Path.Combine(this._package, FileConstants.AppxManifestFile);
                    if (File.Exists(manifestPath))
                    {
                        var result = await this.ExecuteOnExtractedPackage(this._package).ConfigureAwait(false);
                        
                        XDocument document;
                        await using (var fs = File.OpenRead(manifestPath))
                        {
                            document = await XDocument.LoadAsync(fs, LoadOptions.None, CancellationToken.None).ConfigureAwait(false);
                        }

                        var inject = new MsixHeroBrandingInjector();
                        await inject.Inject(document).ConfigureAwait(false);
                        var writer = new AppxDocumentWriter(document);
                        await writer.WriteAsync(manifestPath).ConfigureAwait(false);

                        await this.OnFinished().ConfigureAwait(false);
                        return result;
                    }
                }

                await this.Console.WriteError(string.Format(Resources.Localization.CLI_Executor_Error_PathNotSupported_Format, this._package)).ConfigureAwait(false);
                return StandardExitCodes.ErrorParameter;
            }
            catch (Exception e)
            {
                await this.Console.WriteError(e.Message).ConfigureAwait(false);
                return StandardExitCodes.ErrorGeneric;
            }
        }

        protected abstract Task<int> ExecuteOnExtractedPackage(string directoryPath);

        protected virtual Task<int> Validate()
        {
            return Task.FromResult(0);
        }

        protected virtual Task OnFinished()
        {
            return Task.CompletedTask;
        }

        protected virtual Task OnBegin()
        {
            return Task.CompletedTask;
        }
    }
}
