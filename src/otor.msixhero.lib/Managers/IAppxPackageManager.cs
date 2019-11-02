using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using otor.msixhero.lib.BusinessLayer.Models;
using otor.msixhero.lib.Domain;

namespace otor.msixhero.lib.Managers
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

        Task RunToolInContext(Package package, string toolName);

        Task Run(Package package);

        Task<RegistryMountState> GetRegistryMountState(string installLocation, string packageName);

        Task<RegistryMountState> GetRegistryMountState(Package package);

        Task<IList<Package>> Get(PackageFindMode mode = PackageFindMode.Auto);

        Task Remove(IEnumerable<Package> packages, bool forAllUsers = false, bool preserveAppData = false, IProgress<ProgressData> progress = null);

        Task<IList<Log>> GetLogs(int maxCount);

        Task Add(string filePath, CancellationToken cancellationToken = default, IProgress<ProgressData> progress = default);
    }
}