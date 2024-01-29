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

using System;
using System.IO;
using System.Threading.Tasks;
using OffregLib;

namespace Otor.MsixHero.Registry.Reader
{
    public class OffregRegistryHive : IRegHive
    {
        private readonly OffregHive hive;

        public OffregRegistryHive(OffregHive hive)
        {
            this.hive = hive;
            this.Root = new OffregRegistryKey(null, hive.Root);
        }

        public IRegKey Root { get; }

        public void Dispose()
        {
            this.hive.Dispose();
        }

        public Task Save(string fileName, Version compatibility = null)
        {
            return Task.Run(() =>
            {
                if (File.Exists(fileName))
                {
                    File.Delete(fileName);
                }

                if (compatibility == null)
                {
                    this.hive.SaveHive(fileName, 6, 1);
                }
                else
                {
                    this.hive.SaveHive(fileName, (uint)compatibility.Major, (uint)compatibility.Minor);
                }
            });
        }
    }
}