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
using Newtonsoft.Json;

namespace Otor.MsixHero.Infrastructure.Configuration.ResolvableFolder
{
    public class ResolvablePathConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var date = value as ResolvablePath;
            if (date == null || string.IsNullOrEmpty(date.Compacted))
            {
                writer.WriteValue(string.Empty);
            }
            else
            {
                writer.WriteValue(date.Compacted);
            }
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var result = new ResolvablePath();

            var stringValue = reader.Value as string;
            if (!string.IsNullOrEmpty(stringValue))
            {
                result.Compacted = stringValue;
            }

            return result;
        }

        public override bool CanRead
        {
            get => true;
        }

        public override bool CanWrite
        {
            get => true;
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(ResolvablePath);
        }
    }
}