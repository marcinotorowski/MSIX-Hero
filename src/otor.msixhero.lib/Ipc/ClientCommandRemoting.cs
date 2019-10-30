using System;
using System.Collections.Generic;
using System.IO;
using System.ServiceModel;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Serialization;
using otor.msixhero.lib.BusinessLayer.Commands;
using otor.msixhero.lib.BusinessLayer.Models;
using otor.msixhero.lib.BusinessLayer.State.Enums;

namespace otor.msixhero.lib.Ipc
{
    public class ClientCommandRemoting : IClientCommandRemoting
    {
        private readonly IProcessManager processManager;
        
        public ClientCommandRemoting(IProcessManager processManager)
        {
            this.processManager = processManager;
        }

        public async Task<SelectionDetails> Execute(GetSelectionDetails command, CancellationToken cancellationToken = default)
        {
            return await this.GetOutputFromSelfElevation<GetSelectionDetails, SelectionDetails>(command, cancellationToken).ConfigureAwait(false);
        }

        public async Task<List<Package>> Execute(GetPackages command, CancellationToken cancellationToken = default)
        {
            return await this.GetOutputFromSelfElevation<GetPackages, List<Package>>(command, cancellationToken).ConfigureAwait(false);
        }

        public async Task<bool> Execute(UnmountRegistry command, CancellationToken cancellationToken = default)
        {
            return await this.GetOutputFromSelfElevation<UnmountRegistry, bool>(command, cancellationToken).ConfigureAwait(false);
        }

        public async Task<bool> Execute(RemovePackage command, CancellationToken cancellationToken = default)
        {
            return await this.GetOutputFromSelfElevation<RemovePackage, bool>(command, cancellationToken).ConfigureAwait(false);
        }

        public async Task<List<User>> Execute(GetUsersOfPackage command, CancellationToken cancellationToken = default)
        {
            return await this.GetOutputFromSelfElevation<GetUsersOfPackage, List<User>>(command, cancellationToken).ConfigureAwait(false);
        }

        public async Task<bool> Execute(MountRegistry command, CancellationToken cancellationToken = default)
        {
            return await this.GetOutputFromSelfElevation<MountRegistry, bool>(command, cancellationToken).ConfigureAwait(false);
        }

        public async Task<byte[]> Handle(GetPackages command, IAppxPackageManager packageManager, CancellationToken cancellationToken = default)
        {
            Console.WriteLine("Handling " + command.GetType().Name);
            var packages = new List<Package>(await packageManager.GetPackages(command.Context == PackageContext.AllUsers ? PackageFindMode.AllUsers : PackageFindMode.CurrentUser).ConfigureAwait(false));
            Console.WriteLine("Returning back " + packages.Count + " results.");
            return ReturnAsBytes(packages);
        }

        public async Task<byte[]> Handle(MountRegistry command, IAppxPackageManager pkgManager, CancellationToken cancellationToken = default)
        {
            Console.WriteLine("Handling " + command.GetType().Name);
            await pkgManager.MountRegistry(command.PackageName, command.InstallLocation, command.StartRegedit).ConfigureAwait(false);
            Console.WriteLine("Registry mounted.");
            return ReturnAsBytes(true);
        }

        public async Task<byte[]> Handle(UnmountRegistry command, IAppxPackageManager pkgManager, CancellationToken cancellationToken = default)
        {
            Console.WriteLine("Handling " + command.GetType().Name);
            await pkgManager.UnmountRegistry(command.PackageName).ConfigureAwait(false);
            Console.WriteLine("Registry unmounted.");
            return ReturnAsBytes(true);
        }

        public async Task<byte[]> Handle(GetSelectionDetails command, IAppxPackageManager pkgManager, CancellationToken cancellationToken = default)
        {
            Console.WriteLine("Handling " + command.GetType().Name);
            var users = await pkgManager.GetUsersForPackage(command.FullProductId).ConfigureAwait(false);
            Console.WriteLine("Got " + users.Count + " users.");
            return ReturnAsBytes(
                new SelectionDetails
                {
                    InstalledOn = new InstalledOnDetails
                    {
                        Users = users,
                        Status = ElevationStatus.OK
                    }
                });
        }

        public async Task<byte[]> Handle(GetUsersOfPackage command, IAppxPackageManager pkgManager, CancellationToken cancellationToken = default)
        {
            Console.WriteLine("Handling " + command.GetType().Name);
            var list = await pkgManager.GetUsersForPackage(command.FullProductId).ConfigureAwait(false);
            Console.WriteLine("Returning back " + list.Count + " results.");
            return ReturnAsBytes(list);
        }

        public async Task<byte[]> Handle(RemovePackage command, IAppxPackageManager pkgManager, CancellationToken cancellationToken = default)
        {
            Console.WriteLine("Handling " + command.GetType().Name);
            await pkgManager.RemoveApp(command.Package).ConfigureAwait(false);
            Console.WriteLine("Package uninstalled.");
            return ReturnAsBytes(true);
        }

        public Server GetServerInstance(IAppxPackageManager packageManager)
        {
            var server = new Server();
            server.AddHandler<GetPackages>(action => this.Handle(action, packageManager, CancellationToken.None));
            server.AddHandler<MountRegistry>(action => this.Handle(action, packageManager, CancellationToken.None));
            server.AddHandler<UnmountRegistry>(action => this.Handle(action, packageManager, CancellationToken.None));
            server.AddHandler<GetUsersOfPackage>(action => this.Handle(action, packageManager, CancellationToken.None));
            server.AddHandler<RemovePackage>(action => this.Handle(action, packageManager, CancellationToken.None));
            server.AddHandler<GetSelectionDetails>(action => this.Handle(action, packageManager, CancellationToken.None));

            return server;
        }

        private async Task<TOutput> GetOutputFromSelfElevation<TInput, TOutput>(TInput inputAction, CancellationToken cancellationToken) where TInput : BaseCommand
        {
            var client = new Client(this.processManager);
            var result = await client.Execute<TInput, TOutput>(inputAction, cancellationToken);
            return result;
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
