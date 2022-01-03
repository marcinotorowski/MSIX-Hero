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
using System.IO;
using System.Threading.Tasks;
using Otor.MsixHero.Appx.Packaging;
using Otor.MsixHero.Appx.Packaging.ModificationPackages;
using Otor.MsixHero.Appx.Packaging.ModificationPackages.Entities;
using Otor.MsixHero.Cli.Verbs;
using Otor.MsixHero.Infrastructure.Logging;

namespace Otor.MsixHero.Cli.Executors.Standard
{
    public class NewModPackVerbExecutor : VerbExecutor<NewModPackVerb>
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(NewModPackVerbExecutor));

        private readonly IModificationPackageBuilder modificationPackageBuilder;

        public NewModPackVerbExecutor(NewModPackVerb verb, IModificationPackageBuilder modificationPackageBuilder, IConsole console) : base(verb, console)
        {
            this.modificationPackageBuilder = modificationPackageBuilder;
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

                switch (Path.GetExtension(this.Verb.OutputPath)?.ToLowerInvariant())
                {
                    case FileConstants.MsixExtension:
                    case FileConstants.AppxExtension:
                    {
                        file = this.Verb.OutputPath;
                        action = ModificationPackageBuilderAction.Msix;
                        break;
                    }
                    default:
                    {
                        // ReSharper disable once AssignNullToNotNullAttribute
                        file = Path.Combine(this.Verb.OutputPath, FileConstants.AppxManifestFile);
                        action = ModificationPackageBuilderAction.Manifest;
                        break;
                    }
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

                await this.modificationPackageBuilder.Create(config, file, action).ConfigureAwait(false); 
                await this.Console.WriteSuccess($"Modification package created in {file}.").ConfigureAwait(false);
                return StandardExitCodes.ErrorSuccess;
            }
            catch (Exception e)
            {
                Logger.Error(e);
                await this.Console.WriteError(e.Message).ConfigureAwait(false);
                return StandardExitCodes.ErrorGeneric;
            }
        }
    }
}
