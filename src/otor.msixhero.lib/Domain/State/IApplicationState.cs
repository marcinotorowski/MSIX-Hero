namespace Otor.MsixHero.Lib.Domain.State
{
    public interface IApplicationState
    {
        IVolumeListState Volumes { get; }

        bool IsElevated { get; }

        bool IsSelfElevated { get; }
    }
}