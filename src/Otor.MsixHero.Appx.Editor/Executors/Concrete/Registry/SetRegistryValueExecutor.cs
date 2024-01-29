// MSIX Hero
// Copyright (C) 2024 Marcin Otorowski
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
using Dapplo.Log;
using Otor.MsixHero.Registry.Converter;

namespace Otor.MsixHero.Appx.Editor.Executors.Concrete.Registry
{
    public class SetRegistryValueExecutor : ExtractedAppxExecutor<SetRegistryValue>
    {
        private static readonly LogSource Logger = new();
        public SetRegistryValueExecutor(DirectoryInfo directory) : base(directory)
        {
        }

        public SetRegistryValueExecutor(string directory) : base(directory)
        {
        }

        public override async Task Execute(SetRegistryValue command, CancellationToken cancellationToken = default)
        {
            var converter = new MsixRegistryFileWriter(this.Directory.FullName);
            converter.WriteValue(command.RegistryKey, command.RegistryValueName, command.ValueType, command.Value);
            await converter.Flush().ConfigureAwait(false);
            var target = RegistryPathConverter.ToCanonicalRegistryPath(command.RegistryKey);
            Logger.Info().WriteLine(Resources.Localization.AppxEditor_Registry_RegValueSet_Format, command.RegistryValueName, command.Value, command.ValueType, target.Item1, target.Item2);
        }
    }
}