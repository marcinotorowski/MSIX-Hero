using System.Collections.Generic;
using System.Linq;
using Otor.MsixHero.Appx.Diagnostic.Recommendations.Entities;

namespace Otor.MsixHero.Lib.BusinessLayer.SystemState.ThirdParty
{
    public class ThirdPartyAppProvider : IThirdPartyAppProvider
    {
        public IEnumerable<IThirdPartyApp> ProvideApps()
        {
            var detectors = new List<IThirdPartyAppProvider>
            {
                new MsixHeroAppProvider(),
                new AdvancedInstallerExpressAppProvider(),
                new MsixPackagingToolAppProvider(),
                new RayPackAppProvider()
            };

            return detectors.SelectMany(d => d.ProvideApps()).Where(a => a.AppId != "MSIXHERO").OrderBy(a => a.AppId == "MSIXPKGTOOL" ? "#" : a.Name);
        }
    }
}
