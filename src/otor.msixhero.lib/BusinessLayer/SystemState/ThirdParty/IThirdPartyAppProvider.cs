using System.Collections.Generic;
using otor.msixhero.lib.Domain.SystemState.ThirdParty;

namespace otor.msixhero.lib.BusinessLayer.SystemState.ThirdParty
{
    public interface IThirdPartyAppProvider
    {
        IEnumerable<IThirdPartyApp> ProvideApps();
    }
}