namespace otor.msixhero.lib.Domain.State
{
    public interface IApplicationState
    {
        IPackageListState Packages { get; }

        IVolumeListState Volumes { get; }

        bool IsElevated { get; }

        bool IsSelfElevated { get; }

        ApplicationMode Mode { get; }
    }
}