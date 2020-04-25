using System.Collections.Generic;
using otor.msixhero.lib.Domain.SystemState.ThirdParty;

namespace otor.msixhero.lib.BusinessLayer.SystemState.ThirdParty
{
    public interface IThirdPartyDetector
    {
        IEnumerable<ThirdPartyDetectedApp> DetectApps();
    }
}