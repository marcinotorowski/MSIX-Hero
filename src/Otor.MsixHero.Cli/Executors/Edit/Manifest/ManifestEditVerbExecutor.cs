// MSIX Hero
// Copyright (C) 2021 Marcin Otorowski
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
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using Otor.MsixHero.Appx.Editor.Facades;
using Otor.MsixHero.Appx.Editor.Helpers;
using Otor.MsixHero.Appx.Packaging;
using Otor.MsixHero.Cli.Verbs.Edit;

namespace Otor.MsixHero.Cli.Executors.Edit.Manifest
{
    public abstract class ManifestEditVerbExecutor<T> : BaseEditVerbExecutor<T> where T : BaseEditVerb
    {
        protected ManifestEditVerbExecutor(string package, T verb, IConsole console) : base(package, verb, console)
        {
        }

        protected abstract Task<int> ExecuteOnManifest(XDocument manifest);

        protected sealed override async Task<int> ExecuteOnExtractedPackage(string directoryPath)
        {
            var manifestFile = Path.Combine(directoryPath, FileConstants.AppxManifestFile);
            if (!File.Exists(manifestFile))
            {
                throw new FileNotFoundException($"Manifest file {manifestFile} does not exist.");
            }

            XDocument document;
            int result;
            {
                await using var fs = File.OpenRead(manifestFile);
                document = await XDocument.LoadAsync(fs, LoadOptions.None, CancellationToken.None).ConfigureAwait(false);
                var rootNode = document.Root;
                if (rootNode?.Name.LocalName != "Package")
                {
                    throw new FormatException("XML file must contain root <Package /> element.");
                }

                result = await this.ExecuteOnManifest(document).ConfigureAwait(false);
                var brandingInjector = new MsixHeroBrandingInjector();
                await brandingInjector.Inject(document).ConfigureAwait(false);
            }

            var writer = new AppxDocumentWriter(document);
            await writer.WriteAsync(manifestFile).ConfigureAwait(false);

            return result;
        }
    }
}