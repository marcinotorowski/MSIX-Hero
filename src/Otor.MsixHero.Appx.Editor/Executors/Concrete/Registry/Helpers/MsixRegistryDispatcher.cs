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
using System.Collections.Generic;
using Otor.MsixHero.Registry.Parser;

namespace Otor.MsixHero.Appx.Editor.Executors.Concrete.Registry.Helpers
{
    public class MsixRegistryDispatcher
    {
        public IList<RegistryEntry> Registry { get; } = new List<RegistryEntry>();

        public IList<RegistryEntry> User = new List<RegistryEntry>();

        public IList<RegistryEntry> UserClasses = new List<RegistryEntry>();
        
        public void Add(RegistryEntry entry)
        {
            if (entry.Root == RegistryRoot.HKEY_CLASSES_ROOT)
            {
                // Readdress classes root into local machine.
                entry.Root = RegistryRoot.HKEY_LOCAL_MACHINE;
                entry.Key = "Software\\Classes\\" + entry.Key;
                this.Registry.Add(entry);
            }
            else if (entry.Root == RegistryRoot.HKEY_CURRENT_USER)
            {
                if (entry.Key.StartsWith("software\\classes\\", StringComparison.OrdinalIgnoreCase))
                {
                    this.UserClasses.Add(entry);
                }

                this.User.Add(entry);
                this.Registry.Add(entry);
            }
            else
            {
                this.Registry.Add(entry);
            }
        }

        public void Import(string regFile)
        {
            var regParser = new RegFileParser();
            var parsedKeys = regParser.Parse(regFile);
            
            foreach (var key in parsedKeys)
            {
                this.Add(key);
            }
        }
    }
}