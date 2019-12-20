using System;
using Newtonsoft.Json;

namespace otor.msixhero.lib.Infrastructure.Configuration.ResolvableFolder
{
    public class ResolvableFolderConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var date = value as ResolvableFolder;
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
            var result = new ResolvableFolder();

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
            return objectType == typeof(ResolvableFolder);
        }
    }
}