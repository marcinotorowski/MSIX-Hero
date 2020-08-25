namespace Otor.MsixHero.Lib.Domain.State
{
    public interface IWritableApplicationState
    {
        IWritableVolumeListState Volumes { get; }
        
        bool IsSelfElevated { get; set; }

        bool IsElevated { get; }
    }
}