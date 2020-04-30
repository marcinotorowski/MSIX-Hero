namespace otor.msixhero.lib.Domain.SystemState.ThirdParty
{
    public interface IThirdPartyDetectedApp : IThirdPartyApp
    {
        string Version { get; }
    }
}