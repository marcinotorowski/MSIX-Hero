using otor.msixhero.lib.BusinessLayer.State;

namespace otor.msixhero.lib.BusinessLayer.Reducers
{
    public abstract class BaseSelfElevationReducer<T> : BaseReducer<T> where T : IApplicationState
    {
    }
}