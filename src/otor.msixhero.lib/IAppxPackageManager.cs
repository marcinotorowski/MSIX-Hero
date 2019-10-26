using System.Collections.Generic;
using System.Threading.Tasks;
using otor.msixhero.lib.BusinessLayer.Models;

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
        Task<List<User>> GetUsersForPackage(Package package);

        Task<List<User>> GetUsersForPackage(string packageName);

        Task MountRegistry(Package package, bool startRegedit = false);

        Task MountRegistry(string packageName, string installLocation, bool startRegedit = false);

        Task UnmountRegistry(Package package);

        Task UnmountRegistry(string packageName);

        Task RunTool(Package package, string toolName);

        Task RunApp(Package package);

        Task<RegistryMountState> GetRegistryMountState(string installLocation, string packageName);

        Task<RegistryMountState> GetRegistryMountState(Package package);

        Task<IList<Package>> GetPackages(PackageFindMode mode = PackageFindMode.Auto);
        Task RemoveApp(Package package, bool forAllUsers = false, bool preserveAppData = false);
    }
}