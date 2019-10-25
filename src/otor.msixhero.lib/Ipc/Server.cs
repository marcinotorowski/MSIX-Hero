using System;
using System.Collections.Generic;
using System.IO.Pipes;
using System.Security.Principal;
using System.Threading.Tasks;
using otor.msixhero.lib.Ipc.Commands;
using otor.msixhero.lib.Ipc.Helpers;
using otor.msixhero.lib.Ipc.Streams;

namespace otor.msixhero.lib.Ipc
{
    public class Server
    {
        private readonly string instanceId;

        private readonly Dictionary<Type, Func<BaseCommand, BaseCommand>> handlers = new Dictionary<Type, Func<BaseCommand, BaseCommand>>();
        private readonly HashSet<Type> inputOutputTypes = new HashSet<Type>();

        public Server(string instanceId)
        {
            this.instanceId = instanceId;
        }

        public void AddHandler<T>(Func<T, T> handler) where T : BaseCommand
        {
            this.handlers.Add(typeof(T), command => handler((T)command));
            this.inputOutputTypes.Add(typeof(T));
        }

        public async Task Start()
        {
            try
            {
                var ps = new PipeSecurity();
                var sid = new SecurityIdentifier(WellKnownSidType.WorldSid, null);
                ps.SetAccessRule(new PipeAccessRule(sid, PipeAccessRights.ReadWrite, System.Security.AccessControl.AccessControlType.Allow));

                using (var pipeServer = NamedPipeNative.CreateNamedPipe("msixhero-" + instanceId, ps))
                {
                    while (true)
                    {
                        // var client = await tcpServer.AcceptTcpClientAsync();
                        await pipeServer.WaitForConnectionAsync();

                        var stream = pipeServer;
                        // using var stream = client.GetStream();
                        var binaryWriter = new BinaryStreamProcessor(stream);
                        var binaryReader = new BinaryStreamProcessor(stream);

                        var command = await binaryReader.Read<BaseCommand>();

                        if (this.handlers.TryGetValue(command.GetType(), out var handler))
                        {
                            try
                            {
                                var result = handler(command);
                                await binaryWriter.Write(true).ConfigureAwait(false);
                                await binaryWriter.Write(result).ConfigureAwait(false);
                                await stream.FlushAsync().ConfigureAwait(false);
                            }
                            catch (Exception e)
                            {
                                await binaryWriter.Write(false);
                                await binaryWriter.Write(e.Message);
                                await binaryWriter.Write(e.StackTrace);
                                await stream.FlushAsync().ConfigureAwait(false);
                            }

                            // end of protocol
                            await binaryReader.ReadBoolean().ConfigureAwait(false);
                        }
                        else
                        {
                            throw new NotSupportedException("Not supported command: " + command.GetType().Name);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
    }
}
