namespace otor.msixhero.lib.BusinessLayer.State
{
    public interface IApplicationState
    {
        IPackageListState Packages { get; }    

        ILocalSettings LocalSettings { get; }

        bool IsElevated { get; }

        bool HasSelfElevated { get; }
    }
}