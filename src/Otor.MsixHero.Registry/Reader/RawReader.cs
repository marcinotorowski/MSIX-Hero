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
using System.Threading.Tasks;

namespace Otor.MsixHero.Registry.Reader
{
    public class RawReader
    {
        public async Task<IRegHive> Create(string path)
        {
            using (var embedded = typeof(RawReader).Assembly.GetManifestResourceStream("Otor.MsixHero.Registry.Resources.empty.dat"))
            {
                using (var fs = File.OpenWrite(path))
                {
                    if (embedded == null)
                    {
                        throw new NotSupportedException();
                    }

                    await embedded.CopyToAsync(fs).ConfigureAwait(false);
                }
            }

            return new OffregRegistryHive(OffregLib.OffregHive.Open(path));
        }

        public Task<IRegHive> Create()
        {
            IRegHive hive = new OffregRegistryHive(OffregLib.OffregHive.Create());
            return Task.FromResult(hive);
        }

        public Task<IRegHive> Open(string file)
        {
            if (!File.Exists(file))
            {
                throw new FileNotFoundException("Registry file does not exist.", nameof(file));
            }

            IRegHive hive = new OffregRegistryHive(OffregLib.OffregHive.Open(file));
            return Task.FromResult(hive);
        }
    }
}