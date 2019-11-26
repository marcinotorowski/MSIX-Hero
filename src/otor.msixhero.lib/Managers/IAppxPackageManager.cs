using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using otor.msixhero.lib.BusinessLayer.Models.Logs;
using otor.msixhero.lib.BusinessLayer.Models.Manifest.Full;
using otor.msixhero.lib.BusinessLayer.Models.Packages;
using otor.msixhero.lib.BusinessLayer.Models.Users;
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
        Task<List<User>> GetUsersForPackage(Package package, CancellationToken cancellationToken = default, IProgress<ProgressData> progress = default);

        Task<List<User>> GetUsersForPackage(string packageName, CancellationToken cancellationToken = default, IProgress<ProgressData> progress = default);

        Task MountRegistry(Package package, bool startRegedit = false, CancellationToken cancellationToken = default, IProgress<ProgressData> progress = default);

        Task MountRegistry(string packageName, string installLocation, bool startRegedit = false, CancellationToken cancellationToken = default, IProgress<ProgressData> progress = default);

        Task UnmountRegistry(Package package, CancellationToken cancellationToken = default, IProgress<ProgressData> progress = default);

        Task UnmountRegistry(string packageName, CancellationToken cancellationToken = default, IProgress<ProgressData> progress = default);

        Task RunToolInContext(Package package, string toolPath, string arguments = null, CancellationToken cancellationToken = default, IProgress<ProgressData> progress = default);

        Task RunToolInContext(string packageFamilyName, string appId, string toolPath, string arguments = null, CancellationToken cancellationToken = default, IProgress<ProgressData> progress = default);
        
        Task Run(Package package, CancellationToken cancellationToken = default, IProgress<ProgressData> progress = default);

        Task Run(string packageManifestLocation, string packageFamilyName, CancellationToken cancellationToken = default, IProgress<ProgressData> progress = default);

        Task<RegistryMountState> GetRegistryMountState(string installLocation, string packageName, CancellationToken cancellationToken = default, IProgress<ProgressData> progress = default);

        Task<RegistryMountState> GetRegistryMountState(Package package, CancellationToken cancellationToken = default, IProgress<ProgressData> progress = default);

        Task<IList<Package>> Get(PackageFindMode mode = PackageFindMode.Auto, CancellationToken cancellationToken = default, IProgress<ProgressData> progress = default);

        Task InstallCertificate(string certificateFilePath, CancellationToken cancellationToken = default, IProgress<ProgressData> progress = default);

        Task<AppxPackage> Get(string packageName, CancellationToken cancellationToken = default, IProgress<ProgressData> progress = default);

        Task Remove(IEnumerable<Package> packages, bool forAllUsers = false, bool preserveAppData = false, CancellationToken cancellationToken = default, IProgress<ProgressData> progress = null);

        Task<IList<Log>> GetLogs(int maxCount, CancellationToken cancellationToken = default, IProgress<ProgressData> progress = default);

        Task Add(string filePath, CancellationToken cancellationToken = default, IProgress<ProgressData> progress = default);
    }
}