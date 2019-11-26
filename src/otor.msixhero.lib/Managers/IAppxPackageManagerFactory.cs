namespace otor.msixhero.lib.Managers
{
    public interface IAppxPackageManagerFactory
    {
        IAppxPackageManager GetLocal();

        IAppxPackageManager GetRemote();
    }
}