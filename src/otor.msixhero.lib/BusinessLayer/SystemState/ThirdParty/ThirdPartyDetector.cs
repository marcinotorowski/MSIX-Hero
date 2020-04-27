using System.Collections.Generic;
using System.Linq;
using otor.msixhero.lib.Domain.SystemState.ThirdParty;

namespace otor.msixhero.lib.BusinessLayer.SystemState.ThirdParty
{
    public class ThirdPartyDetector : IThirdPartyDetector
    {
        public IEnumerable<ThirdPartyDetectedApp> DetectApps()
        {
            var detectors = new List<IThirdPartyDetector>
            {
                new MsixHeroDetector(),
                new AdvancedInstallerExpressDetector(),
                new MsixPackagingToolDetector(),
                new RayPackDetector()
            };

            return detectors.SelectMany(d => d.DetectApps()).Where(a => a.AppId != "MSIXHERO").OrderBy(a => a.AppId == "MSIXHERO" ? "#" : a.Name);
        }
    }
}
