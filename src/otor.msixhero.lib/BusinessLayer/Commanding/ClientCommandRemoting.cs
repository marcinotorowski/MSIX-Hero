using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Serialization;
using otor.msixhero.lib.BusinessLayer.Appx;
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
            server.AddHandler<GetPackages>((action, cancellation, progress) => this.Handle(action, packageManager, cancellation));
            server.AddHandler<MountRegistry>((action, cancellation, progress) => this.Handle(action, packageManager, cancellation, progress));
            server.AddHandler<UnmountRegistry>((action, cancellation, progress) => this.Handle(action, packageManager, cancellation, progress));
            server.AddHandler<GetUsersOfPackage>((action, cancellation, progress) => this.Handle(action, packageManager, cancellation, progress));
            server.AddHandler<RemovePackages>((action, cancellation, progress) => this.Handle(action, packageManager, cancellation, progress));
            server.AddHandler<FindUsers>((action, cancellation, progress) => this.Handle(action, packageManager, cancellation));
            server.AddHandler<GetLogs>((action, cancellation, progress) => this.Handle(action, packageManager, cancellation));
            server.AddHandler<InstallCertificate>((action, cancellation, progress) => this.Handle(action, packageManager, cancellation));
            server.AddHandler<RunPackage>((action, cancellation, progress) => this.Handle(action, packageManager, cancellation));
            return server;
        }
        
        public Client GetClientInstance()
        {
            return new Client(this.processManager);
        }

        private async Task Handle(RunPackage command, IAppxPackageManager packageManager, CancellationToken cancellationToken = default, IProgress<ProgressData> progress = default)
        {
            Console.WriteLine("Handling " + command.GetType().Name);
            await packageManager.Run(command.ManifestPath, command.PackageFamilyName, command.ApplicationId, cancellationToken, progress).ConfigureAwait(false);
            Console.WriteLine("Package started.");

        }

        private async Task<byte[]> Handle(GetPackages command, IAppxPackageManager packageManager, CancellationToken cancellationToken = default, IProgress<ProgressData> progress = default)
        {
            Console.WriteLine("Handling " + command.GetType().Name);
            var packages = new List<Package>(await packageManager.Get(command.Context == PackageContext.AllUsers ? PackageFindMode.AllUsers : PackageFindMode.CurrentUser, cancellationToken, progress).ConfigureAwait(false));
            Console.WriteLine("Returning back " + packages.Count + " results.");
            return ReturnAsBytes(packages);
        }

        private async Task Handle(InstallCertificate command, IAppxPackageManager packageManager, CancellationToken cancellationToken = default, IProgress<ProgressData> progress = default)
        {
            Console.WriteLine("Handling " + command.GetType().Name);
            await packageManager.InstallCertificate(command.FilePath, cancellationToken, progress).ConfigureAwait(false);
            Console.WriteLine("Certificate installed.");
        }

        private async Task Handle(MountRegistry command, IAppxPackageManager pkgManager, CancellationToken cancellationToken = default, IProgress<ProgressData> progress = default)
        {
            Console.WriteLine("Handling " + command.GetType().Name + " on " + pkgManager.GetType().Name);
            await pkgManager.MountRegistry(command.PackageName, command.InstallLocation, command.StartRegedit, cancellationToken, progress).ConfigureAwait(false);
            Console.WriteLine("Registry mounted.");
        }

        private async Task<byte[]> Handle(GetLogs command, IAppxPackageManager packageManager, CancellationToken cancellationToken = default, IProgress<ProgressData> progress = default)
        {
            Console.WriteLine("Handling " + command.GetType().Name);
            var logs = await packageManager.GetLogs(command.MaxCount, cancellationToken, progress).ConfigureAwait(false);
            Console.WriteLine("Returning " + logs.Count);
            return ReturnAsBytes(logs);
        }

        private async Task Handle(UnmountRegistry command, IAppxPackageManager pkgManager, CancellationToken cancellationToken = default, IProgress<ProgressData> progress = default)
        {
            Console.WriteLine("Handling " + command.GetType().Name + " on " + pkgManager.GetType().Name);
            await pkgManager.UnmountRegistry(command.PackageName, cancellationToken, progress).ConfigureAwait(false);
            Console.WriteLine("Registry unmounted.");
        }

        private async Task<byte[]> Handle(FindUsers command, IAppxPackageManager pkgManager, CancellationToken cancellationToken = default, IProgress<ProgressData> progress = default)
        {
            Console.WriteLine("Handling " + command.GetType().Name);
            var users = await pkgManager.GetUsersForPackage(command.FullProductId, cancellationToken, progress).ConfigureAwait(false);
            Console.WriteLine("Got " + users.Count + " users.");
            return ReturnAsBytes(
                new FoundUsers
                {
                    Users = users,
                    Status = ElevationStatus.OK
                });
        }

        private async Task<byte[]> Handle(GetUsersOfPackage command, IAppxPackageManager pkgManager, CancellationToken cancellationToken = default, IProgress<ProgressData> progress = default)
        {
            Console.WriteLine("Handling " + command.GetType().Name);
            var list = await pkgManager.GetUsersForPackage(command.FullProductId).ConfigureAwait(false);
            Console.WriteLine("Returning back " + list.Count + " results.");
            return ReturnAsBytes(list);
        }

        private async Task Handle(RemovePackages command, IAppxPackageManager pkgManager, CancellationToken cancellationToken = default, IProgress<ProgressData> progress = default)
        {
            Console.WriteLine("Handling " + command.GetType().Name);
            await pkgManager.Remove(command.Packages, cancellationToken: cancellationToken, progress: progress).ConfigureAwait(false);
            Console.WriteLine("Package uninstalled.");
        }

        private static byte[] ReturnAsBytes<T>(T input)
        {
            var xmlSerializer = new XmlSerializer(typeof(T));
            using var memory = new MemoryStream();
            xmlSerializer.Serialize(memory, input);
            memory.Seek(0, SeekOrigin.Begin);
            return memory.ToArray();
        }
    }
}
