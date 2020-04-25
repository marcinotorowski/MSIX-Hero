using System.Collections.Generic;
using System.Reflection;
using otor.msixhero.lib.Domain.SystemState.ThirdParty;

namespace otor.msixhero.lib.BusinessLayer.SystemState.ThirdParty
{
    public class MsixHeroDetector : IThirdPartyDetector
    {
        public IEnumerable<ThirdPartyDetectedApp> DetectApps()
        {
            var version = (Assembly.GetEntryAssembly() ?? Assembly.GetExecutingAssembly()).GetName().Version;
            yield return new ThirdPartyDetectedApp("MSIXHERO", "MSIX Hero", "Marcin Otorowski", version.ToString(), "https://msixhero.net");
        }
    }
}