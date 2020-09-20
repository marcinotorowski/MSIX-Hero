using System;

namespace Otor.MsixHero.Lib.Proxy.Packaging.Dto
{
    [Serializable]
    public class StopDto : ProxyObject
    {
        public string PackageFullName { get; set; }
    }
}