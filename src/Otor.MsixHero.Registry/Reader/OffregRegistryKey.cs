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

using System.Collections.Generic;
using System.Linq;
using Microsoft.Win32;
using OffregLib;

namespace Otor.MsixHero.Registry.Reader
{
    public class OffregRegistryKey : IRegKey
    {
        private readonly OffregRegistryKey parent;
        private readonly OffregKey offregKey;

        public OffregRegistryKey(OffregRegistryKey parent, OffregKey offregKey)
        {
            this.parent = parent;
            this.offregKey = offregKey;
        }

        public string Name => this.offregKey.Name;

        public string Path => this.offregKey.FullName;

        public IRegKey Parent => this.parent;

        public void Dispose()
        {
            this.offregKey.Dispose();
        }

        public IEnumerable<IRegKey> Keys
        {
            get
            {
                var subKeys = this.offregKey.GetSubKeyNames();
                return subKeys.Select(s => new OffregRegistryKey(this, this.offregKey.OpenSubKey(s)));
            }
        }

        public IEnumerable<IRegValue> Values
        {
            get
            {
                var valueNames = this.offregKey.EnumerateValues();
                return valueNames.Select(v => new OffregRegistryValue(this, v.Name, v.Data, ConvertFromOffreg(v.Type)));
            }
        }

        public IRegKey GetSubKey(string name)
        {
            if (this.offregKey.TryOpenSubKey(name, out var k))
            {
                return new OffregRegistryKey(this, k);
            }

            return null;
        }

        private static RegistryValueKind ConvertFromOffreg(RegValueType reg)
        {
            switch (reg)
            {
                case RegValueType.REG_NONE:
                    return RegistryValueKind.None;
                case RegValueType.REG_SZ:
                    return RegistryValueKind.String;
                case RegValueType.REG_EXPAND_SZ:
                    return RegistryValueKind.ExpandString;
                case RegValueType.REG_BINARY:
                    return RegistryValueKind.Binary;
                case RegValueType.REG_DWORD:
                case RegValueType.REG_DWORD_BIG_ENDIAN:
                    return RegistryValueKind.DWord;
                case RegValueType.REG_MULTI_SZ:
                    return RegistryValueKind.MultiString;
                case RegValueType.REG_QWORD:
                    return RegistryValueKind.QWord;
                default:
                    return RegistryValueKind.Unknown;
            }
        }

        public bool RemoveSubKey(string name)
        {
            if (!this.offregKey.TryOpenSubKey(name, out var k))
            {
                return false;
            }

            using (k)
            {
                k.Delete();
            }

            return true;
        }

        public bool RemoveValue(string name)
        {
            if (!this.offregKey.ValueExist(name))
            {
                return false;
            }

            this.offregKey.DeleteValue(name);
            return true;
        }

        public IRegKey AddSubKey(string name)
        {
            var newSubKey = this.offregKey.CreateSubKey(name);
            return new OffregRegistryKey(this, newSubKey);
        }
        
        public IRegValue SetValue(string name, byte[] binaryValue)
        {
            this.offregKey.SetValue(name, binaryValue);
            return this.GetValue(name);
        }

        public IRegValue SetValue(string name, int dwordValue)
        {
            this.offregKey.SetValue(name, dwordValue);
            return this.GetValue(name);
        }

        public IRegValue SetValue(string name, long qwordValue)
        {
            this.offregKey.SetValue(name, qwordValue);
            return this.GetValue(name);
        }

        public IRegValue SetValue(string name, string stringValue, bool expanded = false)
        {
            this.offregKey.SetValue(name, stringValue, expanded ? RegValueType.REG_EXPAND_SZ : RegValueType.REG_SZ);
            return this.GetValue(name);
        }

        public IRegValue SetValue(string name, string[] multiStringValue)
        {
            this.offregKey.SetValue(name, multiStringValue);
            return this.GetValue(name);
        }

        private IRegValue GetValue(string name)
        {
            if (this.offregKey.ValueExist(name))
            {
                return null;
            }

            var type = this.offregKey.GetValueKind(name);
            var value = this.offregKey.GetValue(name);

            return new OffregRegistryValue(this, name, value, ConvertFromOffreg(type));
        }
    }
}