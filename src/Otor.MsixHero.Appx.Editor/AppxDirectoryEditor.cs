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
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using Otor.MsixHero.Appx.Editor.Commands;
using Otor.MsixHero.Appx.Editor.Commands.Concrete.Files;
using Otor.MsixHero.Appx.Editor.Commands.Concrete.Manifest;
using Otor.MsixHero.Appx.Editor.Commands.Concrete.Registry;
using Otor.MsixHero.Appx.Editor.Executors.Concrete.Files;
using Otor.MsixHero.Appx.Editor.Executors.Concrete.Manifest;
using Otor.MsixHero.Appx.Editor.Executors.Concrete.Registry;
using Otor.MsixHero.Appx.Editor.Helpers;
using Otor.MsixHero.Infrastructure.Logging;

namespace Otor.MsixHero.Appx.Editor
{
    public class AppxDirectoryEditor : IAppxEditor
    {
        private readonly DirectoryInfo directoryInfo;
        public static readonly ILog Log = LogManager.GetLogger(typeof(AppxDirectoryEditor));
        
        private XDocument manifest;

        public AppxDirectoryEditor(DirectoryInfo directoryInfo)
        {
            this.directoryInfo = directoryInfo;
        }

        public AppxDirectoryEditor(string directory) : this(new DirectoryInfo(directory))
        {
        }

        public async Task Begin(CancellationToken cancellationToken = default)
        {
            var path = Path.Combine(this.directoryInfo.FullName, "appxmanifest.xml");
            if (!File.Exists(path))
            {
                throw new FileNotFoundException("Manifest file not found in " + path);
            }

            await using var fs = File.Open(path, FileMode.Open);
            this.manifest = await XDocument.LoadAsync(fs, LoadOptions.None, CancellationToken.None).ConfigureAwait(false);
        }

        public async Task Finish(CancellationToken cancellationToken = default)
        {
            var path = Path.Combine(this.directoryInfo.FullName, "AppxManifest.xml");
            var writer = new AppxDocumentWriter(this.manifest);
            await writer.WriteAsync(path).ConfigureAwait(false);
        }
        
        public async Task Execute(IAppxEditCommand command, CancellationToken cancellationToken = default)
        {
            switch (command)
            {
                case SetPackageIdentity setIdentity:
                {
                    await new SetPackageIdentityExecutor(this.manifest).Execute(setIdentity, cancellationToken).ConfigureAwait(false);
                    break;
                }

                case SetPackageProperties setPackageDisplayInformation:
                {
                    await new SetPackagePropertiesExecutor(this.manifest).Execute(setPackageDisplayInformation, cancellationToken).ConfigureAwait(false);
                    break;
                }

                case AddCapability addCapability:
                {
                    await new AddCapabilityExecutor(this.manifest).Execute(addCapability, cancellationToken).ConfigureAwait(false);
                    break;
                }

                case DeleteFile deleteFile:
                {
                    await new DeleteFileExecutor(this.directoryInfo).Execute(deleteFile, cancellationToken).ConfigureAwait(false);
                    break;
                }

                case AddFile replaceFile:
                {
                    await new AddFileExecutor(this.directoryInfo).Execute(replaceFile, cancellationToken).ConfigureAwait(false);
                    break;
                }

                case SetBuildMetaData addBuildMetaData:
                {
                    await new SetBuildMetaDataExecutor(this.manifest).Execute(addBuildMetaData, cancellationToken).ConfigureAwait(false);
                    break;
                }

                case DeleteRegistryKey deleteRegistryKey:
                {
                    await new DeleteRegistryKeyExecutor(this.directoryInfo).Execute(deleteRegistryKey, cancellationToken).ConfigureAwait(false);
                    break;
                }

                case SetRegistryKey setRegistryKey:
                {
                    await new SetRegistryKeyExecutor(this.directoryInfo).Execute(setRegistryKey, cancellationToken).ConfigureAwait(false);
                    break;
                }

                case SetRegistryValue setRegistryValue:
                {
                    await new SetRegistryValueExecutor(this.directoryInfo).Execute(setRegistryValue, cancellationToken).ConfigureAwait(false);
                    break;
                }

                case ImportLocalRegistryKey importLocalRegistryKey:
                {
                    await new ImportLocalRegistryKeyExecutor(this.directoryInfo).Execute(importLocalRegistryKey, cancellationToken).ConfigureAwait(false);
                    break;
                }

                case ImportRegistryFile importRegistryFile:
                {
                    await new ImportRegistryFileExecutor(this.directoryInfo).Execute(importRegistryFile, cancellationToken).ConfigureAwait(false);
                    break;
                }

                default:
                    throw new NotSupportedException($"The command {command.GetType().Name} is not supported.");
            }
        }
    }
}
