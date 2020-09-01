using System;

namespace Otor.MsixHero.Lib.Proxy.Packaging.Dto
{
    [Serializable]
    public class DeprovisionDto : ProxyObject
    {
        public DeprovisionDto()
        {
        }

        public DeprovisionDto(string packageFamilyName) : this()
        {
            this.PackageFamilyName = packageFamilyName;
        }

        public string PackageFamilyName { get; set;  }
    }
}