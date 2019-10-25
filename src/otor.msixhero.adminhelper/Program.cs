using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Net.Sockets;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using otor.msixhero.lib;
using otor.msixhero.lib.BusinessLayer.Actions;
using otor.msixhero.lib.BusinessLayer.State.Enums;
using otor.msixhero.lib.Ipc;
using otor.msixhero.lib.Ipc.Helpers;

namespace otor.msixhero.adminhelper
{
    public class Program
    {
        static void Main(string[] args)
        {
            if (args.Length > 1 && args[0] == "--pipe")
            {
                StartPipe(args[1]).Wait();
            }
        }

        private static async Task<byte[]> Handle(GetPackages command)
        {
            var pkgManager = new AppxPackageManager();
            var packages = new List<Package>(await pkgManager.GetPackages(command.Context == PackageContext.AllUsers ? PackageFindMode.AllUsers : PackageFindMode.CurrentUser));
            return ReturnAsBytes(packages);
        }

        private static async Task<byte[]> Handle(MountRegistry command)
        {
            var pkgManager = new AppxPackageManager();
            await pkgManager.MountRegistry(command.PackageName, command.InstallLocation, command.StartRegedit);
            return ReturnAsBytes(true);
        }

        private static async Task<byte[]> Handle(UnmountRegistry command)
        {
            var pkgManager = new AppxPackageManager();
            await pkgManager.UnmountRegistry(command.PackageName);
            return ReturnAsBytes(true);
        }

        private static async Task<byte[]> Handle(FindUsersOfPackage command)
        {
            var pkgManager = new AppxPackageManager();
            var list = await pkgManager.GetUsersForPackage(command.FullProductId);
            return ReturnAsBytes((List<string>)list);
        }

        private static async Task StartPipe(string instanceId)
        {
            try
            {
                var server = new Server(instanceId);
                server.AddHandler<GetPackages>(Handle);
                server.AddHandler<MountRegistry>(Handle);
                server.AddHandler<UnmountRegistry>(Handle);
                server.AddHandler<FindUsersOfPackage>(Handle);
                await server.Start();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        private static async Task ListAllUserPackages(BinaryReader reader, BinaryWriter writer)
        {
            try
            {
                Console.WriteLine("Getting all tasks");
                var pkgManager = new AppxPackageManager();
                var allTasks = await pkgManager.GetPackages(PackageFindMode.AllUsers);
                var xmlSerializer = new XmlSerializer(typeof(List<Package>));

                using var memStream = new MemoryStream();
                xmlSerializer.Serialize(memStream, allTasks);

                memStream.Seek(0, SeekOrigin.Begin);
                Console.WriteLine(System.Text.Encoding.UTF8.GetString(memStream.ToArray()));

                var arr = memStream.ToArray();
                memStream.Seek(0, SeekOrigin.Begin);
                Console.WriteLine(allTasks.Count + "elements in " + arr.Length + " bytes");

                Console.WriteLine("Return true");
                writer.Write(true);
                writer.Write(arr.Length);
                writer.Write(arr);
            }
            catch (Exception e)
            {
                // in case of failure, write false, then name of the exception and then the stack trace
                Console.WriteLine("Return false");
                writer.Write(false);
                Console.WriteLine(e.ToString());
                writer.Write(e.GetType().Name);
                writer.Write(e.StackTrace);
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
