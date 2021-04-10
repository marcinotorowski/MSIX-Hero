using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using YamlDotNet.Serialization;

namespace Otor.MsixHero.Winget.Yaml.Entities
{
    public class YamlStringEnumConverter : IYamlTypeConverter
    {
        public bool Accepts(Type type) => type.IsEnum;

        public object ReadYaml(IParser parser, Type type)
        {
            var parsedEnum = parser.Consume<Scalar>();
            var serializableValues = type.GetMembers()
                .Select(member => new KeyValuePair<string, MemberInfo>(
                    member.GetCustomAttributes<EnumMemberAttribute>(true).Select(enumMemberAttribute => enumMemberAttribute.Value).FirstOrDefault(), member))
                .Where(pa => !string.IsNullOrEmpty(pa.Key))
                .ToDictionary(pa => pa.Key, pa => pa.Value, StringComparer.OrdinalIgnoreCase);

            if (!serializableValues.ContainsKey(parsedEnum.Value))
            {
                throw new YamlException(parsedEnum.Start, parsedEnum.End, $"Value '{parsedEnum.Value}' not found in enum '{type.Name}'");
            }

            return Enum.Parse(type, serializableValues[parsedEnum.Value].Name);
        }

        public void WriteYaml(IEmitter emitter, object value, Type type)
        {
            var valueString = value?.ToString();
            if (valueString == null)
            {
                return;
            }

            var enumMember = type.GetMember(valueString).FirstOrDefault();
            if (enumMember == null)
            {
                emitter.Emit(new Scalar(valueString));
                return;
            }

            var yamlValue = enumMember.GetCustomAttributes<EnumMemberAttribute>(true).Select(ema => ema.Value).FirstOrDefault() ?? value.ToString();
            if (string.IsNullOrEmpty(yamlValue))
            {
                emitter.Emit(new Scalar(valueString));
                return;
            }

            emitter.Emit(new Scalar(yamlValue));
        }
    }
}
