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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Otor.MsixHero.Registry.Converter;
using Otor.MsixHero.Registry.Reader;

namespace Otor.MsixHero.Appx.Editor.Executors.Concrete.Registry.Helpers
{
    public class MsixRegistryFileKeyDeleter
    {
        private readonly string fileRegistry, fileUser, fileUserClasses;
        private readonly HashSet<string> deletedKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        public MsixRegistryFileKeyDeleter(string registryDirectory)
        {
            this.fileRegistry = Path.Combine(registryDirectory, "Registry.dat");
            this.fileUser = Path.Combine(registryDirectory, "User.dat");
            this.fileUserClasses = Path.Combine(registryDirectory, "User.Classes.dat");
        }

        public MsixRegistryFileKeyDeleter(string registryDatFile, string userDatFile, string userClassesDatFile)
        {
            this.fileRegistry = registryDatFile;
            this.fileUser = userDatFile;
            this.fileUserClasses = userClassesDatFile;
        }

        public void RemoveKey(string key)
        {
            this.deletedKeys.Add(key);
        }

        public async Task<bool> Flush()
        {
            var rawReader = new RawReader();
            IRegHive hiveRegistry = null;
            IRegHive hiveUser = null;
            IRegHive hiveUserClasses = null;

            if (File.Exists(this.fileRegistry))
            {
                hiveRegistry = await rawReader.Open(this.fileRegistry).ConfigureAwait(false);
            }

            if (File.Exists(this.fileUser))
            {
                hiveUser = await rawReader.Open(this.fileUser).ConfigureAwait(false);
            }

            if (File.Exists(this.fileUserClasses))
            {
                hiveUserClasses = await rawReader.Open(this.fileUserClasses).ConfigureAwait(false);
            }

            if (hiveUserClasses == null && hiveRegistry == null && hiveUser == null)
            {
                return false;
            }

            var foundAnythingRegistry = false;
            var foundAnythingUser = false;
            var foundAnythingUserClasses = false;
            
            using (hiveRegistry)
            {
                using (hiveUser)
                {
                    using (hiveUserClasses)
                    {
                        foreach (var key in this.deletedKeys)
                        {
                            var msix = RegistryPathConverter.ToMsixRegistryPath(key);
                            
                            if (hiveRegistry != null)
                            {
                                using var foundKey = this.GetExistingRegistryKey(hiveRegistry.Root, msix);
                                if (foundKey != null)
                                {
                                    foundAnythingRegistry = true;
                                    RemoveRecurse(foundKey);
                                }
                            }

                            if (hiveUser != null)
                            {
                                using var foundKey = this.GetExistingRegistryKey(hiveUser.Root, msix);

                                if (foundKey != null)
                                {
                                    foundAnythingUser = true;
                                    RemoveRecurse(foundKey);
                                }
                            }

                            if (hiveUserClasses != null)
                            {
                                using var foundKey = this.GetExistingRegistryKey(hiveUserClasses.Root, msix);

                                if (foundKey != null)
                                {
                                    foundAnythingUserClasses = true;
                                    RemoveRecurse(foundKey);
                                }
                            }
                        }

                        if (foundAnythingRegistry)
                        {
                            await hiveRegistry.Save(this.fileRegistry).ConfigureAwait(false);
                        }

                        if (foundAnythingUser)
                        {
                            await hiveUser.Save(this.fileUser).ConfigureAwait(false);
                        }

                        if (foundAnythingUserClasses)
                        {
                            await hiveUserClasses.Save(this.fileUserClasses).ConfigureAwait(false);
                        }
                    }
                }
            }

            this.deletedKeys.Clear();
            return foundAnythingRegistry || foundAnythingUser || foundAnythingUserClasses;
        }

        private static void RemoveRecurse(IRegKey foundKey)
        {
            foreach (var subKeyName in foundKey.Keys.Select(k => k.Name).ToArray())
            {
                using var subKey = foundKey.GetSubKey(subKeyName);
                RemoveRecurse(subKey);
            }

            // remove all values
            foreach (var value in foundKey.Values.ToArray())
            {
                foundKey.RemoveValue(value.Name);
            }

            foundKey.Parent.RemoveSubKey(foundKey.Name);
        }

        private IRegKey GetExistingRegistryKey(IRegKey regKey, string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                return regKey;
            }

            var pos = path.IndexOf('\\');

            if (pos == -1)
            {
                var child = regKey.GetSubKey(path);
                if (child != null)
                {
                    return child;
                }

                return null;
            }
            else
            {
                var child = regKey.GetSubKey(path.Substring(0, pos));
                if (child == null)
                {
                    return null;
                }

                return this.GetExistingRegistryKey(child, path.Substring(pos + 1));
            }
        }
    }
}