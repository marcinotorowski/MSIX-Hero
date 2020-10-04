using System;
using System.IO;
using System.Threading.Tasks;
using Otor.MsixHero.Appx.Packaging.Manifest;
using Otor.MsixHero.Appx.Packaging.Manifest.FileReaders;
using Otor.MsixHero.Appx.Packaging.ModificationPackages;
using Otor.MsixHero.Appx.Packaging.ModificationPackages.Entities;
using Otor.MsixHero.Appx.Signing;
using Otor.MsixHero.Cli.Verbs;
using Otor.MsixHero.Infrastructure.Logging;

namespace Otor.MsixHero.Cli.Executors
{
    public class NewModPackVerbExecutor : VerbExecutor<NewModPackVerb>
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(NewModPackVerbExecutor));

        private readonly IAppxContentBuilder appxContentBuilder;

        public NewModPackVerbExecutor(NewModPackVerb verb, IAppxContentBuilder appxContentBuilder, IConsole console) : base(verb, console)
        {
            this.appxContentBuilder = appxContentBuilder;
        }

        public override async Task<int> Execute()
        {
            try
            {
                string file;
                ModificationPackageBuilderAction action;

                var config = new ModificationPackageConfig
                {
                    Name = this.Verb.Name,
                    Publisher = this.Verb.PublisherName,
                    DisplayName = this.Verb.DisplayName,
                    DisplayPublisher = this.Verb.PublisherDisplayName,
                    Version = this.Verb.Version,
                    IncludeVfsFolders = this.Verb.CopyFolderStructure
                };

                if (string.Equals(".msix", Path.GetExtension(this.Verb.OutputPath), StringComparison.OrdinalIgnoreCase))
                {
                    file = this.Verb.OutputPath;
                    action = ModificationPackageBuilderAction.Msix;
                }
                else
                {
                    // ReSharper disable once AssignNullToNotNullAttribute
                    file = Path.Combine(this.Verb.OutputPath, "AppxManifest.xml");
                    action = ModificationPackageBuilderAction.Manifest;
                }

                config.ParentPackagePath = this.Verb.ParentPackagePath;
                config.ParentName = this.Verb.ParentName;
                config.ParentPublisher = this.Verb.ParentPublisher;

                if (!string.IsNullOrEmpty(this.Verb.IncludeRegFile))
                {
                    config.IncludeRegistry = new FileInfo(this.Verb.IncludeRegFile);
                }

                if (!string.IsNullOrEmpty(this.Verb.IncludeFolder))
                {
                    config.IncludeFolder = new DirectoryInfo(this.Verb.IncludeFolder);
                }

                await this.appxContentBuilder.Create(config, file, action).ConfigureAwait(false); 
                await this.Console.WriteSuccess($"Modification package created in {file}.").ConfigureAwait(false);
                return 0;
            }
            catch (Exception e)
            {
                Logger.Error(e);
                await this.Console.WriteError(e.Message).ConfigureAwait(false);
                return 1;
            }
        }
    }
}
