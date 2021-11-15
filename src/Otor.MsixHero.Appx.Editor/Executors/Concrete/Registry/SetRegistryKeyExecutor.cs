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
using Otor.MsixHero.Registry.Converter;

namespace Otor.MsixHero.Appx.Editor.Executors.Concrete.Registry
{
    public class SetRegistryKeyExecutor : ExtractedAppxExecutor<SetRegistryKey>
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(SetRegistryKeyExecutor));

        public SetRegistryKeyExecutor(DirectoryInfo directory) : base(directory)
        {
        }

        public SetRegistryKeyExecutor(string directory) : base(directory)
        {
        }

        public override async Task Execute(SetRegistryKey command, CancellationToken cancellationToken = default)
        {
            var converter = new MsixRegistryFileWriter(this.Directory.FullName);
            converter.WriteKey(command.RegistryKey);
            await converter.Flush().ConfigureAwait(false);
            var target = RegistryPathConverter.ToCanonicalRegistryPath(command.RegistryKey);
            Logger.Info($"Registry key {target.Item1}\\{target.Item2} has been set.");
        }
    }
}