using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Serialization;
using otor.msixhero.lib.BusinessLayer.Commands.Developer;
using otor.msixhero.lib.BusinessLayer.Commands.Grid;
using otor.msixhero.lib.BusinessLayer.Commands.Manager;
using otor.msixhero.lib.BusinessLayer.Commands.Signing;
using otor.msixhero.lib.BusinessLayer.Models;
using otor.msixhero.lib.BusinessLayer.Models.Packages;
using otor.msixhero.lib.BusinessLayer.State.Enums;
using otor.msixhero.lib.Domain;
using otor.msixhero.lib.Managers;

namespace otor.msixhero.lib.Ipc
{
    public class ClientCommandRemoting : IClientCommandRemoting
    {
        private readonly IProcessManager processManager;
        
        public ClientCommandRemoting(IProcessManager processManager)
        {
            this.processManager = processManager;
        }

        public Server GetServerInstance(IAppxPackageManager packageManager, IAppxSigningManager signingManager)
        {
            var server = new Server();
            server.AddHandler<GetPackages>((action, cancellation, progress) => this.Handle(action, packageManager, cancellation));
            server.AddHandler<MountRegistry>((action, cancellation, progress) => this.Handle(action, packageManager, cancellation, progress));
            server.AddHandler<UnmountRegistry>((action, cancellation, progress) => this.Handle(action, packageManager, cancellation, progress));
            server.AddHandler<GetUsersOfPackage>((action, cancellation, progress) => this.Handle(action, packageManager, cancellation, progress));
            server.AddHandler<RemovePackages>((action, cancellation, progress) => this.Handle(action, packageManager, cancellation, progress));
            server.AddHandler<FindUsers>((action, cancellation, progress) => this.Handle(action, packageManager, cancellation));
            server.AddHandler<GetLogs>((action, cancellation, progress) => this.Handle(action, packageManager, cancellation));
            server.AddHandler<InstallCertificate>((action, cancellation, progress) => this.Handle(action, signingManager, cancellation));
            return server;
        }
        
        public Client GetClientInstance()
        {
            return new Client(this.processManager);
        }

        public async Task<byte[]> Handle(GetPackages command, IAppxPackageManager packageManager, CancellationToken cancellationToken = default, IProgress<ProgressData> progress = default)
        {
            Console.WriteLine("Handling " + command.GetType().Name);
            var packages = new List<Package>(await packageManager.Get(command.Context == PackageContext.AllUsers ? PackageFindMode.AllUsers : PackageFindMode.CurrentUser).ConfigureAwait(false));
            Console.WriteLine("Returning back " + packages.Count + " results.");
            return ReturnAsBytes(packages);
        }

        public async Task<byte[]> Handle(InstallCertificate command, IAppxSigningManager signingManager, CancellationToken cancellationToken = default, IProgress<ProgressData> progress = default)
        {
            Console.WriteLine("Handling " + command.GetType().Name);
            await signingManager.InstallCertificate(command.FilePath, cancellationToken, progress).ConfigureAwait(false);
            Console.WriteLine("Certificate installed.");
            return ReturnAsBytes(true);
        }

        public async Task<byte[]> Handle(MountRegistry command, IAppxPackageManager pkgManager, CancellationToken cancellationToken = default, IProgress<ProgressData> progress = default)
        {
            Console.WriteLine("Handling " + command.GetType().Name);
            await pkgManager.MountRegistry(command.PackageName, command.InstallLocation, command.StartRegedit).ConfigureAwait(false);
            Console.WriteLine("Registry mounted.");
            return ReturnAsBytes(true);
        }

        private async Task<byte[]> Handle(GetLogs command, IAppxPackageManager packageManager, CancellationToken cancellationToken = default, IProgress<ProgressData> progress = default)
        {
            Console.WriteLine("Handling " + command.GetType().Name);
            var logs = await packageManager.GetLogs(command.MaxCount).ConfigureAwait(false);
            Console.WriteLine("Returning " + logs.Count);
            return ReturnAsBytes(logs);
        }

        public async Task<byte[]> Handle(UnmountRegistry command, IAppxPackageManager pkgManager, CancellationToken cancellationToken = default, IProgress<ProgressData> progress = default)
        {
            Console.WriteLine("Handling " + command.GetType().Name);
            await pkgManager.UnmountRegistry(command.PackageName).ConfigureAwait(false);
            Console.WriteLine("Registry unmounted.");
            return ReturnAsBytes(true);
        }

        public async Task<byte[]> Handle(FindUsers command, IAppxPackageManager pkgManager, CancellationToken cancellationToken = default, IProgress<ProgressData> progress = default)
        {
            Console.WriteLine("Handling " + command.GetType().Name);
            var users = await pkgManager.GetUsersForPackage(command.FullProductId).ConfigureAwait(false);
            Console.WriteLine("Got " + users.Count + " users.");
            return ReturnAsBytes(
                new FoundUsers
                {
                    Users = users,
                    Status = ElevationStatus.OK
                });
        }

        public async Task<byte[]> Handle(GetUsersOfPackage command, IAppxPackageManager pkgManager, CancellationToken cancellationToken = default, IProgress<ProgressData> progress = default)
        {
            Console.WriteLine("Handling " + command.GetType().Name);
            var list = await pkgManager.GetUsersForPackage(command.FullProductId).ConfigureAwait(false);
            Console.WriteLine("Returning back " + list.Count + " results.");
            return ReturnAsBytes(list);
        }

        public async Task<byte[]> Handle(RemovePackages command, IAppxPackageManager pkgManager, CancellationToken cancellationToken = default, IProgress<ProgressData> progress = default)
        {
            Console.WriteLine("Handling " + command.GetType().Name);

            await pkgManager.Remove(command.Packages).ConfigureAwait(false);
            Console.WriteLine("Package uninstalled.");
            return ReturnAsBytes(true);
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
