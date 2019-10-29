using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Xml.Serialization;
using otor.msixhero.lib;
using otor.msixhero.lib.BusinessLayer.Commands;
using otor.msixhero.lib.BusinessLayer.State.Enums;
using otor.msixhero.lib.Ipc;

namespace otor.msixhero.adminhelper
{
    public class Program
    {
        static void Main(string[] args)
        {
            if (args.Length > 0 && args[0] == "--selfElevate")
            {
                StartPipe().GetAwaiter().GetResult();
            }
            else
            {
                Environment.ExitCode = 1;
            }
        }

        private static async Task<byte[]> Handle(GetPackages command)
        {
            Console.WriteLine("Handling " + command.GetType().Name);
            var pkgManager = new AppxPackageManager();
            var packages = new List<Package>(await pkgManager.GetPackages(command.Context == PackageContext.AllUsers ? PackageFindMode.AllUsers : PackageFindMode.CurrentUser).ConfigureAwait(false));
            Console.WriteLine("Returning back " + packages.Count + " results.");
            return ReturnAsBytes(packages);
        }

        private static async Task<byte[]> Handle(MountRegistry command)
        {
            Console.WriteLine("Handling " + command.GetType().Name);
            var pkgManager = new AppxPackageManager();
            await pkgManager.MountRegistry(command.PackageName, command.InstallLocation, command.StartRegedit).ConfigureAwait(false);
            Console.WriteLine("Registry mounted.");
            return ReturnAsBytes(true);
        }

        private static async Task<byte[]> Handle(UnmountRegistry command)
        {
            Console.WriteLine("Handling " + command.GetType().Name);
            var pkgManager = new AppxPackageManager();
            await pkgManager.UnmountRegistry(command.PackageName).ConfigureAwait(false);
            Console.WriteLine("Registry unmounted.");
            return ReturnAsBytes(true);
        }

        private static async Task<byte[]> Handle(GetSelectionDetails command)
        {
            Console.WriteLine("Handling " + command.GetType().Name);
            var pkgManager = new AppxPackageManager();
            var users = await pkgManager.GetUsersForPackage(command.FullProductId).ConfigureAwait(false);
            Console.WriteLine("Got " + users.Count + " users.");
            return ReturnAsBytes(users);
        }

        private static async Task<byte[]> Handle(GetUsersOfPackage command)
        {
            Console.WriteLine("Handling " + command.GetType().Name);
            var pkgManager = new AppxPackageManager();
            var list = await pkgManager.GetUsersForPackage(command.FullProductId).ConfigureAwait(false);
            Console.WriteLine("Returning back " + list.Count + " results.");
            return ReturnAsBytes(list);
        }

        private static async Task<byte[]> Handle(RemovePackage command)
        {
            Console.WriteLine("Handling " + command.GetType().Name);
            var pkgManager = new AppxPackageManager();
            await pkgManager.RemoveApp(command.Package).ConfigureAwait(false);
            Console.WriteLine("Package uninstalled.");
            return ReturnAsBytes(true);
        }

        private static async Task StartPipe()
        {
            try
            {
                var server = new Server();
                server.AddHandler<GetPackages>(Handle);
                server.AddHandler<MountRegistry>(Handle);
                server.AddHandler<UnmountRegistry>(Handle);
                server.AddHandler<GetUsersOfPackage>(Handle);
                server.AddHandler<RemovePackage>(Handle);
                server.AddHandler<GetSelectionDetails>(Handle);
                await server.Start();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
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
