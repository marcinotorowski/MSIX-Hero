using System.Collections.Generic;
using System.Threading.Tasks;

namespace otor.msixhero.lib
{
    public enum PackageFindMode
    {
        Auto,
        CurrentUser,
        AllUsers
    }
    public interface IAppxPackageManager
    {
        Task MountRegistry(Package package, bool startRegedit = false);

        Task UnmountRegistry(Package package);

        Task RunTool(Package package, string toolName);

        Task RunApp(Package package);

        Task<RegistryMountState> GetRegistryMountState(string installLocation, string packageName);

        Task<RegistryMountState> GetRegistryMountState(Package package);

        Task<IList<Package>> GetPackages(PackageFindMode mode = PackageFindMode.Auto);
    }
}