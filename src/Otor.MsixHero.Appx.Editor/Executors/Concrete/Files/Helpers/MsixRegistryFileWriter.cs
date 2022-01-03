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
using Microsoft.Win32;
using Otor.MsixHero.Appx.Editor.Executors.Concrete.Registry.Helpers;
using Otor.MsixHero.Registry.Converter;
using Otor.MsixHero.Registry.Parser;
using Otor.MsixHero.Registry.Reader;
using ValueType = Otor.MsixHero.Registry.Parser.ValueType;

namespace Otor.MsixHero.Appx.Editor.Executors.Concrete.Files.Helpers
{
    public class MsixRegistryFileWriter
    {
        private readonly string fileRegistry, fileUser, fileUserClasses;
        private readonly MsixRegistryDispatcher msixRegistryFacade = new MsixRegistryDispatcher();

        public MsixRegistryFileWriter(string registryDirectory)
        {
            this.fileRegistry = Path.Combine(registryDirectory, "Registry.dat");
            this.fileUser = Path.Combine(registryDirectory, "User.dat");
            this.fileUserClasses = Path.Combine(registryDirectory, "User.Classes.dat");
        }

        public MsixRegistryFileWriter(string registryDatFile, string userDatFile, string userClassesDatFile)
        {
            this.fileRegistry = registryDatFile;
            this.fileUser = userDatFile;
            this.fileUserClasses = userClassesDatFile;
        }
        
        public async Task<bool> Flush()
        {
            var reader = new RawReader();

            if (this.msixRegistryFacade.Registry.Any())
            {
                IRegHive hive;

                if (!File.Exists(this.fileRegistry))
                {
                    if (!Directory.Exists(Path.GetDirectoryName(this.fileRegistry)))
                    {
                        // ReSharper disable once AssignNullToNotNullAttribute
                        Directory.CreateDirectory(Path.GetDirectoryName(this.fileRegistry));
                    }

                    hive = await reader.Create().ConfigureAwait(false);
                }
                else
                {
                    hive = await reader.Open(this.fileRegistry).ConfigureAwait(false);
                }

                using (hive)
                {
                    this.CommitEntries(hive.Root, this.msixRegistryFacade.Registry);
                    await hive.Save(this.fileRegistry).ConfigureAwait(false);
                }
            }

            if (this.msixRegistryFacade.User.Any())
            {
                IRegHive hive;

                if (!File.Exists(this.fileUser))
                {
                    if (!Directory.Exists(Path.GetDirectoryName(this.fileUser)))
                    {
                        // ReSharper disable once AssignNullToNotNullAttribute
                        Directory.CreateDirectory(Path.GetDirectoryName(this.fileUser));
                    }

                    hive = await reader.Create().ConfigureAwait(false);
                }
                else
                {
                    hive = await reader.Open(this.fileUser).ConfigureAwait(false);
                }

                using (hive)
                {
                    this.CommitEntries(hive.Root, this.msixRegistryFacade.User);
                    await hive.Save(this.fileUser).ConfigureAwait(false);
                }
            }

            if (this.msixRegistryFacade.UserClasses.Any())
            {
                IRegHive hive;

                if (!File.Exists(this.fileUserClasses))
                {
                    if (!Directory.Exists(Path.GetDirectoryName(this.fileUserClasses)))
                    {
                        // ReSharper disable once AssignNullToNotNullAttribute
                        Directory.CreateDirectory(Path.GetDirectoryName(this.fileUserClasses));
                    }

                    hive = await reader.Create().ConfigureAwait(false);
                }
                else
                {
                    hive = await reader.Open(this.fileUserClasses).ConfigureAwait(false);
                }

                using (hive)
                {
                    this.CommitEntries(hive.Root, this.msixRegistryFacade.UserClasses);
                    await hive.Save(this.fileUserClasses).ConfigureAwait(false);
                }
            }

            var hasChanges = this.msixRegistryFacade.Registry.Any() || this.msixRegistryFacade.User.Any() || this.msixRegistryFacade.UserClasses.Any();

            this.msixRegistryFacade.Registry.Clear();
            this.msixRegistryFacade.User.Clear();
            this.msixRegistryFacade.UserClasses.Clear();

            return hasChanges;
        }

        public void ImportLocalRegistryKey(string registryPath)
        {
            if (registryPath.IndexOf('\\') == -1)
            {
                throw new ArgumentException("The path must be a registry path.", nameof(registryPath));
            }

            var data = RegistryPathConverter.ToCanonicalRegistryPath(registryPath);
            
            RegistryKey sourceRegistryHive;
            
            switch (data.Item1)
            {
                case RegistryRoot.HKEY_CLASSES_ROOT:
                    sourceRegistryHive = RegistryKey.OpenBaseKey(RegistryHive.ClassesRoot, RegistryView.Default);
                    break;
                case RegistryRoot.HKEY_LOCAL_MACHINE:
                    sourceRegistryHive = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Default);
                    break;
                case RegistryRoot.HKEY_CURRENT_USER:
                    sourceRegistryHive = RegistryKey.OpenBaseKey(RegistryHive.CurrentUser, RegistryView.Default);
                    break;
                default:
                    throw new NotSupportedException($"Registry hive {data.Item1} is not supported.");
            }

            using (sourceRegistryHive)
            {
                var sourcePath = registryPath.Substring(registryPath.IndexOf('\\') + 1);

                using var sourceRegistryKey = sourceRegistryHive.OpenSubKey(sourcePath);
                this.WriteKeyToFacade(sourceRegistryKey);
            }
        }

        public void ImportRegFile(string regFile)
        {
            this.msixRegistryFacade.Import(regFile);
        }

        public void WriteValue(RegistryRoot root, string key, string name, ValueType type, object value)
        {
            this.msixRegistryFacade.Add(new RegistryEntry
            {
                Key = key,
                Name = name,
                Root = root,
                Type = type,
                Value = value
            });
        }

        public void WriteValue(string registryKey, string name, ValueType type, object value)
        {
            var converted = RegistryPathConverter.ToCanonicalRegistryPath(registryKey);
            this.WriteValue(converted.Item1, converted.Item2, name, type, value);
        }

        public void WriteKey(RegistryRoot root, string registryKey)
        {
            this.msixRegistryFacade.Add(new RegistryEntry
            {
                Key = registryKey,
                Root = root
            });
        }

        public void WriteKey(string registryKey)
        {
            var converted = RegistryPathConverter.ToCanonicalRegistryPath(registryKey);
            this.WriteKey(converted.Item1, converted.Item2);
        }

        private void WriteKeyToFacade(RegistryKey registryKey)
        {
            var data = RegistryPathConverter.ToCanonicalRegistryPath(registryKey.Name);
            msixRegistryFacade.Add(new RegistryEntry
                {
                    Key = data.Item2,
                    Root = data.Item1
                });

            foreach (var valueName in registryKey.GetValueNames())
            {
                ValueType valueType;
                switch (registryKey.GetValueKind(valueName))
                {
                    case RegistryValueKind.None:
                    case RegistryValueKind.Unknown:
                        valueType = ValueType.None;
                        break;
                    case RegistryValueKind.ExpandString:
                        valueType = ValueType.Expandable;
                        break;
                    case RegistryValueKind.Binary:
                        valueType = ValueType.Binary;
                        break;
                    case RegistryValueKind.DWord:
                        valueType = ValueType.DWord;
                        break;
                    case RegistryValueKind.MultiString:
                        valueType = ValueType.Multi;
                        break;
                    case RegistryValueKind.QWord:
                        valueType = ValueType.QWord;
                        break;
                    default:
                        valueType = ValueType.String;
                        break;
                }

                msixRegistryFacade.Add(new RegistryEntry
                {
                    Key = data.Item2,
                    Value = registryKey.GetValue(valueName),
                    Name = valueName,
                    Root = data.Item1,
                    Type = valueType
                });
            }
        }

        private void CommitEntries(IRegKey rootKey, IEnumerable<RegistryEntry> entries)
        {
            foreach (var entry in entries)
            {
                var actualKey = RegistryPathConverter.ToMsixRegistryPath(entry);

                if (entry.Name != null && entry.Value == null)
                {
                    var lastUnit = actualKey.Substring(0, actualKey.LastIndexOf('\\'));
                    var parent = this.GetExistingRegistryKey(rootKey, lastUnit);
                    if (parent == null)
                    {
                        // don't create a key if we are just about to remove the value...
                        continue;
                    }
                }
                
                var key = this.GetOrCreateRegistryKey(rootKey, actualKey);

                if (entry.Name != null || entry.Value != null)
                {
                    if (!string.IsNullOrEmpty(entry.Name))
                    {
                        if (entry.Value == null)
                        {
                            key.RemoveValue(entry.Name);
                        }
                        else
                        {
                            switch (entry.Type)
                            {
                                case ValueType.Default:
                                    break;
                                case ValueType.String:
                                    key.SetValue(entry.Name, RegistryValueConverter.ToMsixFormat((string)entry.Value));
                                    break;
                                case ValueType.DWord:
                                    var val = (long)Convert.ChangeType(entry.Value, typeof(long));
                                    if (val > int.MaxValue)
                                    {
                                        key.SetValue(entry.Name, val);
                                    }
                                    else
                                    {
                                        key.SetValue(entry.Name, (int)val);
                                    }

                                    break;
                                case ValueType.QWord:
                                    key.SetValue(entry.Name, (long)Convert.ChangeType(entry.Value, typeof(long)));
                                    break;
                                case ValueType.Multi:
                                    key.SetValue(entry.Name, RegistryValueConverter.ToMsixFormat((string[])entry.Value));
                                    break;
                                case ValueType.Expandable:
                                    key.SetValue(entry.Name, RegistryValueConverter.ToMsixFormat((string)entry.Value));
                                    break;
                                case ValueType.Binary:
                                    key.SetValue(entry.Name, (byte[])entry.Value);
                                    break;
                                case ValueType.DWordBigEndian:
                                    key.SetValue(entry.Name, (int)Convert.ChangeType(entry.Value, typeof(int)));
                                    break;
                            }
                        }
                    }
                    else if (entry.Value is string sourceStringValue)
                    {
                        key.SetValue(string.Empty, sourceStringValue);
                    }
                }
            }
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

        private IRegKey GetOrCreateRegistryKey(IRegKey regKey, string path)
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

                return regKey.AddSubKey(path);
            }
            else
            {
                var child = regKey.GetSubKey(path.Substring(0, pos));
                if (child == null)
                {
                    child = regKey.AddSubKey(path.Substring(0, pos));
                }

                return this.GetOrCreateRegistryKey(child, path.Substring(pos + 1));
            }
        }
    }
}
