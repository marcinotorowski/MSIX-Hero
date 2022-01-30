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

using System.Globalization;
using Newtonsoft.Json;

namespace Otor.MsixHero.Elevation;

public class JsonSerializer
{
    public string? Serialize(object? objectToSerialize)
    {
        if (objectToSerialize == null)
        {
            return null;
        }

        if (objectToSerialize is CultureInfo cultureInfo)
        {
            return cultureInfo.ToString();
        }

        return JsonConvert.SerializeObject(objectToSerialize, Formatting.None);
    }

    public T? Deserialize<T>(string? serializedString)
    {
        return (T?)this.Deserialize(typeof(T), serializedString);
    }

    public object? Deserialize(Type type, string? serializedString)
    {
        if (serializedString == null)
        {
            return null;
        }

        if (type == typeof(CultureInfo))
        {
            return CultureInfo.GetCultureInfo(serializedString);
        }

        return JsonConvert.DeserializeObject(serializedString, type);
    }
}