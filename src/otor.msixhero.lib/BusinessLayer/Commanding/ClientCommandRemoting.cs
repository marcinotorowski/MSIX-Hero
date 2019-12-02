using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using otor.msixhero.lib.BusinessLayer.Appx;
using otor.msixhero.lib.Domain.Appx.Logs;
using otor.msixhero.lib.Domain.Appx.Packages;
using otor.msixhero.lib.Domain.Appx.Users;
using otor.msixhero.lib.Domain.Commands.Developer;
using otor.msixhero.lib.Domain.Commands.Grid;
using otor.msixhero.lib.Domain.Commands.Manager;
using otor.msixhero.lib.Domain.Commands.Signing;
using otor.msixhero.lib.Domain.State;
using otor.msixhero.lib.Infrastructure.Interop;
using otor.msixhero.lib.Infrastructure.Ipc;
using otor.msixhero.lib.Infrastructure.Progress;

namespace otor.msixhero.lib.BusinessLayer.Commanding
{
    public class ClientCommandRemoting : IClientCommandRemoting
    {
        private readonly IProcessManager processManager;
        
        public ClientCommandRemoting(IProcessManager processManager)
        {
            this.processManager = processManager;
        }

        public Server GetServerInstance(IAppxPackageManager packageManager)
        {
            var server = new Server();
            server.AddHandler<GetPackages, List<Package>>((action, cancellation, progress) => this.ProcessAndReturn(action, packageManager, cancellation));
            server.AddHandler<MountRegistry>((action, cancellation, progress) => this.Process(action, packageManager, cancellation, progress));
            server.AddHandler<UnmountRegistry>((action, cancellation, progress) => this.Process(action, packageManager, cancellation, progress));
            server.AddHandler<GetUsersOfPackage, List<User>>((action, cancellation, progress) => this.ProcessAndReturn(action, packageManager, cancellation, progress));
            server.AddHandler<RemovePackages>((action, cancellation, progress) => this.Process(action, packageManager, cancellation, progress));
            server.AddHandler<FindUsers, List<User>>((action, cancellation, progress) => this.ProcessAndReturn(action, packageManager, cancellation));
            server.AddHandler<GetLogs>((action, cancellation, progress) => this.ProcessAndReturn(action, packageManager, cancellation));
            server.AddHandler<InstallCertificate>((action, cancellation, progress) => this.Process(action, packageManager, cancellation));
            server.AddHandler<RunPackage>((action, cancellation, progress) => this.Process(action, packageManager, cancellation));
            return server;
        }
        
        public Client GetClientInstance()
        {
            return new Client(this.processManager);
        }

        private async Task Process(RunPackage command, IAppxPackageManager packageManager, CancellationToken cancellationToken = default, IProgress<ProgressData> progress = default)
        {
            Console.WriteLine("Handling " + command.GetType().Name);
            await packageManager.Run(command.ManifestPath, command.PackageFamilyName, command.ApplicationId, cancellationToken, progress).ConfigureAwait(false);
            Console.WriteLine("Package started.");
        }

        private async Task<List<Package>> ProcessAndReturn(GetPackages command, IAppxPackageManager packageManager, CancellationToken cancellationToken = default, IProgress<ProgressData> progress = default)
        {
            Console.WriteLine("Handling " + command.GetType().Name);
            var packages = new List<Package>(await packageManager.Get(command.Context == PackageContext.AllUsers ? PackageFindMode.AllUsers : PackageFindMode.CurrentUser, cancellationToken, progress).ConfigureAwait(false));
            Console.WriteLine("Returning back " + packages.Count + " results.");
            return packages;
        }

        private async Task Process(InstallCertificate command, IAppxPackageManager packageManager, CancellationToken cancellationToken = default, IProgress<ProgressData> progress = default)
        {
            Console.WriteLine("Handling " + command.GetType().Name);
            await packageManager.InstallCertificate(command.FilePath, cancellationToken, progress).ConfigureAwait(false);
            Console.WriteLine("Certificate installed.");
        }

        private async Task Process(MountRegistry command, IAppxPackageManager pkgManager, CancellationToken cancellationToken = default, IProgress<ProgressData> progress = default)
        {
            Console.WriteLine("Handling " + command.GetType().Name + " on " + pkgManager.GetType().Name);
            await pkgManager.MountRegistry(command.PackageName, command.InstallLocation, command.StartRegedit, cancellationToken, progress).ConfigureAwait(false);
            Console.WriteLine("Registry mounted.");
        }

        private async Task<List<Log>> ProcessAndReturn(GetLogs command, IAppxPackageManager packageManager, CancellationToken cancellationToken = default, IProgress<ProgressData> progress = default)
        {
            Console.WriteLine("Handling " + command.GetType().Name);
            var logs = await packageManager.GetLogs(command.MaxCount, cancellationToken, progress).ConfigureAwait(false);
            Console.WriteLine("Returning " + logs.Count);
            return logs;
        }

        private async Task Process(UnmountRegistry command, IAppxPackageManager pkgManager, CancellationToken cancellationToken = default, IProgress<ProgressData> progress = default)
        {
            Console.WriteLine("Handling " + command.GetType().Name + " on " + pkgManager.GetType().Name);
            await pkgManager.UnmountRegistry(command.PackageName, cancellationToken, progress).ConfigureAwait(false);
            Console.WriteLine("Registry unmounted.");
        }

        private async Task<List<User>> ProcessAndReturn(FindUsers command, IAppxPackageManager pkgManager, CancellationToken cancellationToken = default, IProgress<ProgressData> progress = default)
        {
            Console.WriteLine("Handling " + command.GetType().Name);
            var users = await pkgManager.GetUsersForPackage(command.FullProductId, cancellationToken, progress).ConfigureAwait(false);
            Console.WriteLine("Got " + users.Count + " users.");
            return users;
        }

        private async Task<List<User>> ProcessAndReturn(GetUsersOfPackage command, IAppxPackageManager pkgManager, CancellationToken cancellationToken = default, IProgress<ProgressData> progress = default)
        {
            Console.WriteLine("Handling " + command.GetType().Name);
            var list = await pkgManager.GetUsersForPackage(command.FullProductId, cancellationToken).ConfigureAwait(false);
            Console.WriteLine("Returning back " + list.Count + " results.");
            return list;
        }

        private async Task Process(RemovePackages command, IAppxPackageManager pkgManager, CancellationToken cancellationToken = default, IProgress<ProgressData> progress = default)
        {
            Console.WriteLine("Handling " + command.GetType().Name);
            await pkgManager.Remove(command.Packages, cancellationToken: cancellationToken, progress: progress).ConfigureAwait(false);
            Console.WriteLine("Package uninstalled.");
        }
    }
}
