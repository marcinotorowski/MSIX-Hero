namespace otor.msixhero.lib.Domain.SystemState.ThirdParty
{
    public interface IThirdPartyApp
    {
        string AppId { get; }

        string Name { get; }

        string Publisher { get; }

        string Website { get; }
    }
}