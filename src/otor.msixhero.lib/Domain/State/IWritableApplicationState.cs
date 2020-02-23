namespace otor.msixhero.lib.Domain.State
{
    public interface IWritableApplicationState
    {
        IWritablePackageListState Packages { get; }

        IWritableVolumeListState Volumes { get; }
        
        bool IsSelfElevated { get; set; }

        bool IsElevated { get; }

        ApplicationMode Mode { get; set; }
    }
}