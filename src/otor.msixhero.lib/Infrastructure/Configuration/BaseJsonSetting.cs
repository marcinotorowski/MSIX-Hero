using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace otor.msixhero.lib.Infrastructure.Configuration
{
    public class BaseJsonSetting
    {
        [JsonExtensionData]
        // ReSharper disable once InconsistentNaming
        protected IDictionary<string, JToken> additionalData;
    }
}