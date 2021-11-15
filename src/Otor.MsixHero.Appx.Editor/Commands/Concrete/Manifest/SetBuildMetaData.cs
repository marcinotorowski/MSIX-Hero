using System;
using System.Collections.Generic;
using System.Linq;

namespace Otor.MsixHero.Appx.Editor.Commands.Concrete.Manifest
{
    public class SetBuildMetaData : IAppxEditCommand
    {
        public SetBuildMetaData(IDictionary<string, string> values)
        {
            this.Values = values ?? new Dictionary<string, string>();
        }

        public SetBuildMetaData(string key, string value)
        {
            this.Values = new Dictionary<string, string>
            {
                { key, value }
            };
        }

        public SetBuildMetaData(IReadOnlyDictionary<string, Version> versionComponents)
        {
            this.Values = versionComponents.ToDictionary(vc => vc.Key, vc => vc.Value.ToString());
        }

        public SetBuildMetaData(string component, Version version) : this(component, version.ToString())
        {
        }

        public IDictionary<string, string> Values { get; }

        public bool OnlyCreateNew { get; set; }
    }
}
