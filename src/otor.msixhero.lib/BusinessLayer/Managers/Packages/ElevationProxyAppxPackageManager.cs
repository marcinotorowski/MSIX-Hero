using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using otor.msixhero.lib.BusinessLayer.Appx;
using otor.msixhero.lib.Domain.Appx.Logs;
using otor.msixhero.lib.Domain.Appx.Manifest.Full;
using otor.msixhero.lib.Domain.Appx.Packages;
using otor.msixhero.lib.Domain.Appx.Users;
using otor.msixhero.lib.Domain.Commands.Packages.Developer;
using otor.msixhero.lib.Domain.Commands.Packages.Grid;
using otor.msixhero.lib.Domain.Commands.Packages.Manager;
using otor.msixhero.lib.Infrastructure.Helpers;
using otor.msixhero.lib.Infrastructure.Ipc;
using otor.msixhero.lib.Infrastructure.Progress;

namespace otor.msixhero.lib.BusinessLayer.Managers.Packages
{
    public class ElevationProxyAppxPackageManager : IAppxPackageManager
    {
        private readonly IElevatedClient client;

        public ElevationProxyAppxPackageManager(IElevatedClient client)
        {
            this.client = client;
        }

        public async Task<List<User>> GetUsersForPackage(InstalledPackage package, bool forceElevation, CancellationToken cancellationToken = default, IProgress<ProgressData> progress = default)
        {
            if (!forceElevation && !await UserHelper.IsAdministratorAsync(cancellationToken).ConfigureAwait(false))
            {
                return null;
            }

            var cmd = new FindUsers
            {
                Source = package.PackageId,
                ForceElevation = forceElevation
            };

            return await this.client.GetExecuted(cmd, cancellationToken, progress).ConfigureAwait(false);
        }

        public async Task<List<User>> GetUsersForPackage(string packageName, bool forceElevation, CancellationToken cancellationToken = default, IProgress<ProgressData> progress = default)
        {
            if (!forceElevation && !await UserHelper.IsAdministratorAsync(cancellationToken).ConfigureAwait(false))
            {
                return null;
            }

            var cmd = new FindUsers
            {
                Source = packageName,
                ForceElevation = forceElevation
            };
            
            return await this.client.GetExecuted(cmd, cancellationToken, progress).ConfigureAwait(false);
        }

        public Task Deprovision(string packageFamilyName, CancellationToken cancellationToken = default, IProgress<ProgressData> progress = default)
        {
            var cmd = new Deprovision
            {
                PackageFamilyName = packageFamilyName
            };

            return this.client.Execute(cmd, cancellationToken, progress);
        }

        public Task RunToolInContext(InstalledPackage package, string toolPath, string arguments = null, CancellationToken cancellationToken = default, IProgress<ProgressData> progress = default)
        {
            var cmd = new RunToolInPackage
            {
                PackageFamilyName = package.PackageFamilyName,
                AppId = package.PackageId,
                Arguments = arguments,
                ToolPath = toolPath,
                AsAdmin = true
            };

            return this.client.Execute(cmd, cancellationToken, progress);
        }

        public Task RunToolInContext(string packageFamilyName, string appId, string toolPath, string arguments = null, CancellationToken cancellationToken = default, IProgress<ProgressData> progress = default)
        {
            var cmd = new RunToolInPackage
            {
                PackageFamilyName = packageFamilyName,
                AppId = appId,
                Arguments = arguments,
                ToolPath = toolPath,
                AsAdmin = true
            };

            return this.client.Execute(cmd, cancellationToken, progress);
        }

        public Task Run(InstalledPackage package, string appId = null, CancellationToken cancellationToken = default, IProgress<ProgressData> progress = default)
        {
            var cmd = new RunPackage
            {
                PackageFamilyName = package.PackageFamilyName,
                ApplicationId = appId
            };

            return this.client.Execute(cmd, cancellationToken, progress);
        }

        public Task Run(string packageManifestLocation, string packageFamilyName, string appId = null, CancellationToken cancellationToken = default, IProgress<ProgressData> progress = default)
        {
            var cmd = new RunPackage
            {
                PackageFamilyName = packageFamilyName,
                ManifestPath = packageManifestLocation,
                ApplicationId = appId
            };

            return this.client.Execute(cmd, cancellationToken, progress);
        }

        public Task<List<InstalledPackage>> GetInstalledPackages(PackageFindMode mode = PackageFindMode.Auto, CancellationToken cancellationToken = default, IProgress<ProgressData> progress = default)
        {
            var cmd = new GetPackages
            {
                Context = mode == PackageFindMode.CurrentUser ? PackageContext.CurrentUser : PackageContext.AllUsers
            };

            return this.client.GetExecuted(cmd, cancellationToken, progress);
        }

        public Task<AppxPackage> GetByIdentity(string packageName, PackageFindMode mode = PackageFindMode.CurrentUser, CancellationToken cancellationToken = default, IProgress<ProgressData> progress = default)
        {
            var cmd = new GetPackageDetails
            {
                Context = mode == PackageFindMode.CurrentUser ? PackageContext.CurrentUser : PackageContext.AllUsers,
                Source = packageName
            };

            return this.client.GetExecuted(cmd, cancellationToken, progress);
        }

        public Task<AppxPackage> GetByManifestPath(string manifestPath, PackageFindMode mode = PackageFindMode.CurrentUser, CancellationToken cancellationToken = default, IProgress<ProgressData> progress = default)
        {
            var cmd = new GetPackageDetails
            {
                Context = mode == PackageFindMode.CurrentUser ? PackageContext.CurrentUser : PackageContext.AllUsers,
                Source = manifestPath
            };

            return this.client.GetExecuted(cmd, cancellationToken, progress);
        }

        public Task Remove(IReadOnlyCollection<InstalledPackage> packages, bool forAllUsers = false, bool preserveAppData = false, CancellationToken cancellationToken = default, IProgress<ProgressData> progress = null)
        {
            var cmd = new RemovePackages
            {
                Packages = packages.ToList(),
                Context = forAllUsers ? PackageContext.AllUsers : PackageContext.CurrentUser,
                RemoveAppData = !preserveAppData
            };

            return this.client.Execute(cmd, cancellationToken, progress);
        }

        public Task<List<Log>> GetLogs(int maxCount, CancellationToken cancellationToken = default, IProgress<ProgressData> progress = default)
        {
            var cmd = new GetLogs
            {
                MaxCount = maxCount
            };

            return this.client.GetExecuted(cmd, cancellationToken, progress);
        }

        public Task Add(string filePath, AddPackageOptions options = (AddPackageOptions) 0, CancellationToken cancellationToken = default, IProgress<ProgressData> progress = default)
        {
            var cmd = new AddPackage
            {
                FilePath = filePath,
                AllUsers = options.HasFlag(AddPackageOptions.AllUsers),
                AllowDowngrade = options.HasFlag(AddPackageOptions.AllowDowngrade),
                KillRunningApps = options.HasFlag(AddPackageOptions.KillRunningApps)
            };

            return this.client.Execute(cmd, cancellationToken, progress);
        }
    }
}