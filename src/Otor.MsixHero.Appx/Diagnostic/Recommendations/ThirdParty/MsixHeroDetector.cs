using System.Collections.Generic;
using System.Reflection;
using Otor.MsixHero.Appx.Diagnostic.Recommendations.Entities;

namespace Otor.MsixHero.Lib.BusinessLayer.SystemState.ThirdParty
{
    public class MsixHeroAppProvider : IThirdPartyAppProvider
    {
        public IEnumerable<IThirdPartyApp> ProvideApps()
        {
            var version = (Assembly.GetEntryAssembly() ?? Assembly.GetExecutingAssembly()).GetName().Version;
            // ReSharper disable once PossibleNullReferenceException
            yield return new ThirdPartyDetectedApp("MSIXHERO", "MSIX Hero", "Marcin Otorowski", version.ToString(), "https://msixhero.net");
        }
    }
}