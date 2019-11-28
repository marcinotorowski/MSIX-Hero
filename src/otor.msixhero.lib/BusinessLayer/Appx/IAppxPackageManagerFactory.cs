namespace otor.msixhero.lib.BusinessLayer.Appx
{
    public interface IAppxPackageManagerFactory
    {
        IAppxPackageManager GetLocal();

        IAppxPackageManager GetRemote();
    }
}