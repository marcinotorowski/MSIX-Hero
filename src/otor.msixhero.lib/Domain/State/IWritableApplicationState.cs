namespace otor.msixhero.lib.Domain.State
{
    public interface IWritableApplicationState
    {
        IWritablePackageListState Packages { get; }
        
        bool IsSelfElevated { get; set; }

        bool IsElevated { get; }
    }
}