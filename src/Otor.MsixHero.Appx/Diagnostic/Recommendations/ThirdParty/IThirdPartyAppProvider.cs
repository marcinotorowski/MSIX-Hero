using System.Collections.Generic;
using Otor.MsixHero.Appx.Diagnostic.Recommendations.Entities;

namespace Otor.MsixHero.Lib.BusinessLayer.SystemState.ThirdParty
{
    public interface IThirdPartyAppProvider
    {
        IEnumerable<IThirdPartyApp> ProvideApps();
    }
}