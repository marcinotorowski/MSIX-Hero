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
using System.Collections.Generic;

namespace Otor.MsixHero.Registry.Reader
{
    public interface IRegKey : IDisposable
    {
        string Name { get; }

        string Path { get; }

        IRegKey Parent { get; }

        IEnumerable<IRegKey> Keys { get; }

        IEnumerable<IRegValue> Values { get; }
        
        bool RemoveSubKey(string name);

        bool RemoveValue(string name);

        IRegKey GetSubKey(string name);
        
        IRegKey AddSubKey(string name);

        IRegValue SetValue(string name, byte[] binaryValue);

        IRegValue SetValue(string name, int dwordValue);

        IRegValue SetValue(string name, long qwordValue);

        IRegValue SetValue(string name, string stringValue, bool expanded = false);

        IRegValue SetValue(string name, string[] multiStringValue);

    }
}
