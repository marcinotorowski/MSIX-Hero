using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace otor.msixhero.lib.Domain.Appx.Psf
{
    public abstract class JsonElement
    {
        [JsonExtensionData]
        // ReSharper disable once UnusedMember.Global
        // ReSharper disable once InconsistentNaming
        public IDictionary<string, JToken> rawValues;
    }
}