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

using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Otor.MsixHero.Appx.Editor.Commands.Concrete.Registry;
using Otor.MsixHero.Appx.Editor.Executors.Concrete.Registry.Helpers;
using Dapplo.Log;
using Otor.MsixHero.Registry.Converter;

namespace Otor.MsixHero.Appx.Editor.Executors.Concrete.Registry
{
    public class DeleteRegistryKeyExecutor : ExtractedAppxExecutor<DeleteRegistryKey>
    {
        private static readonly LogSource Logger = new();
        public DeleteRegistryKeyExecutor(DirectoryInfo directory) : base(directory)
        {
        }

        public DeleteRegistryKeyExecutor(string directory) : base(directory)
        {
        }

        public override async Task Execute(DeleteRegistryKey command, CancellationToken cancellationToken = default)
        {
            var target = RegistryPathConverter.ToCanonicalRegistryPath(command.RegistryKey);
            Logger.Info().WriteLine(Resources.Localization.AppxEditor_Registry_RemovingKey_Format, target.Item1, target.Item2);
            var regFileDeleter = new MsixRegistryFileKeyDeleter(this.Directory.FullName);
            regFileDeleter.RemoveKey(command.RegistryKey);
            if (!await regFileDeleter.Flush().ConfigureAwait(false))
            {
                Logger.Warn().WriteLine(Resources.Localization.AppxEditor_Warn_NoChangesDueToMissingKey_Format, target.Item1, target.Item2);
            }
        }
    }
}