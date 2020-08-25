using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Otor.MsixHero.Appx.Psf.Entities
{
    public abstract class JsonElement
    {
        [JsonExtensionData]
        // ReSharper disable once UnusedMember.Global
        // ReSharper disable once InconsistentNaming
        public IDictionary<string, JToken> rawValues;
    }
}