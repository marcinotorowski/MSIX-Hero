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
using otor.msixhero.lib.Ipc;
using otor.msixhero.lib.Ipc.Commands;
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

        private static GetPackagesCommand Handle(GetPackagesCommand command)
        {
            var pkgManager = new AppxPackageManager();
            var packages = pkgManager.GetPackages(command.Mode).Result.ToList();
            command.Result = packages;
            return command;
        }

        private static GetPackagesCommand Handle(MountRegistryCommand command)
        {
            throw new NotImplementedException();
        }

        private static GetPackagesCommand Handle(UnmountRegistryCommand command)
        {
            throw new NotImplementedException();
        }

        private static async Task StartPipe(string instanceId)
        {
            try
            {
                var server = new Server(instanceId);
                server.AddHandler<GetPackagesCommand>(Handle);
                await server.Start();
                return;

                // var tcpServer = new TcpListener(45678);
                // tcpServer.Start();

                var ps = new PipeSecurity();
                var sid = new SecurityIdentifier(WellKnownSidType.WorldSid, null);
                // var me = System.Security.Principal.WindowsIdentity.GetCurrent().User;

                ps.SetAccessRule(new PipeAccessRule(sid, PipeAccessRights.ReadWrite, System.Security.AccessControl.AccessControlType.Allow));

                // using var pipeServer = NamedPipeNative.CreateNamedPipe("msixhero-" + instanceId, ps);
                using var pipeServer = NamedPipeNative.CreateNamedPipe("msixhero-" + instanceId, ps);
                
                // new NamedPipeServerStream()

                // using var pipeServer = new NamedPipeServerStream("msixhero-" + instanceId, PipeDirection.InOut, 10, PipeTransmissionMode.Byte, PipeOptions.WriteThrough, 1024, 1024);

                // pipeServer.SetAccessControl(ps);

                while (true)
                {
                    // var client = await tcpServer.AcceptTcpClientAsync();
                    await pipeServer.WaitForConnectionAsync();

                    var stream = pipeServer;
                    // using var stream = client.GetStream();
                    using var binaryWriter = new BinaryWriter(stream, Encoding.UTF8);
                    using var binaryReader = new BinaryReader(stream, Encoding.UTF8);

                    var command = binaryReader.ReadString();
                    Console.WriteLine("Got command " + command);
                    switch (command)
                    {
                        case "listAllUserPackages":
                            await ListAllUserPackages(binaryReader, binaryWriter);
                            break;
                        default:
                            break;
                    }
                }
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
    }
}
