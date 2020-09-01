using System.Collections.Generic;
using Otor.MsixHero.Appx.Diagnostic.Recommendations.Entities;

namespace Otor.MsixHero.Appx.Diagnostic.Recommendations.ThirdParty
{
    public interface IThirdPartyAppProvider
    {
        IEnumerable<IThirdPartyApp> ProvideApps();
    }
}