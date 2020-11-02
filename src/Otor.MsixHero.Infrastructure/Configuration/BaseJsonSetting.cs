using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Otor.MsixHero.Infrastructure.Configuration
{
    public class BaseJsonSetting
    {
        [JsonExtensionData]
        // ReSharper disable once InconsistentNaming
        protected IDictionary<string, JToken> additionalData;
    }
}