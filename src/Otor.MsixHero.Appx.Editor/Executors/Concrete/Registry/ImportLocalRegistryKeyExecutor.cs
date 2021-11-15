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

using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Otor.MsixHero.Appx.Editor.Commands.Concrete.Registry;
using Otor.MsixHero.Appx.Editor.Executors.Concrete.Files.Helpers;
using Otor.MsixHero.Infrastructure.Logging;

namespace Otor.MsixHero.Appx.Editor.Executors.Concrete.Registry
{
    public class ImportLocalRegistryKeyExecutor : ExtractedAppxExecutor<ImportLocalRegistryKey>
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(ImportLocalRegistryKeyExecutor));

        public ImportLocalRegistryKeyExecutor(DirectoryInfo directory) : base(directory)
        {
        }

        public ImportLocalRegistryKeyExecutor(string directory) : base(directory)
        {
        }

        public override async Task Execute(ImportLocalRegistryKey command, CancellationToken cancellationToken = default)
        {
            var regWriter = new MsixRegistryFileWriter(this.Directory.FullName);
            regWriter.ImportLocalRegistryKey(command.RegistryKey);
            
            if (await regWriter.Flush().ConfigureAwait(false))
            {
                Logger.Info("Registry key has been successfully imported.");
            }
            else
            {
                Logger.Warn("No new entries have been imported.");
            }
        }
    }
}
