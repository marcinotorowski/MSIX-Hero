using System;
using System.Collections.Generic;
using System.IO.Pipes;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;
using otor.msixhero.lib.BusinessLayer.Commands;
using otor.msixhero.lib.Ipc.Helpers;
using otor.msixhero.lib.Ipc.Streams;

namespace otor.msixhero.lib.Ipc
{
    public class Server
    {
        private readonly Dictionary<Type, Func<BaseCommand, Task<byte[]>>> handlers = new Dictionary<Type, Func<BaseCommand, Task<byte[]>>>();
        private readonly HashSet<Type> inputOutputTypes = new HashSet<Type>();

        public void AddHandler<T>(Func<T, Task<byte[]>> handler) where T : BaseCommand
        {
            this.handlers.Add(typeof(T), command => handler((T)command));
            this.inputOutputTypes.Add(typeof(T));
        }

        public async Task Start(CancellationToken cancellationToken = default)
        {
            try
            {
                var ps = new PipeSecurity();
                var sid = new SecurityIdentifier(WellKnownSidType.WorldSid, null);
                ps.SetAccessRule(new PipeAccessRule(sid, PipeAccessRights.ReadWrite, System.Security.AccessControl.AccessControlType.Allow));

                // ReSharper disable once StringLiteralTypo
                using (var stream = NamedPipeNative.CreateNamedPipe("msixhero", ps))
                {
                    while (true)
                    {
                        // var client = await tcpServer.AcceptTcpClientAsync();
                        await stream.WaitForConnectionAsync(cancellationToken);

                        // using var stream = client.GetStream();
                        var binaryWriter = new BinaryStreamProcessor(stream);
                        var binaryReader = new BinaryStreamProcessor(stream);

                        var command = await binaryReader.Read<BaseCommand>(cancellationToken);

                        try
                        {
                            if (this.handlers.TryGetValue(command.GetType(), out var handler))
                            {
                                try
                                {
                                    var result = await handler(command);
                                    await binaryWriter.Write(true, cancellationToken).ConfigureAwait(false);
                                    await binaryWriter.Write(result, cancellationToken).ConfigureAwait(false);
                                    await stream.FlushAsync(cancellationToken).ConfigureAwait(false);
                                }
                                catch (OperationCanceledException)
                                {
                                }
                                catch (Exception e)
                                {
                                    await binaryWriter.Write(false, cancellationToken);
                                    await binaryWriter.Write(e.Message, cancellationToken);
                                    await binaryWriter.Write(e.ToString(), cancellationToken);
                                    await stream.FlushAsync(cancellationToken).ConfigureAwait(false);
                                }

                                // end of protocol
                                await binaryReader.ReadBoolean(cancellationToken).ConfigureAwait(false);
                            }
                            else
                            {
                                await binaryWriter.Write(false, cancellationToken);
                                await binaryWriter.Write("The command " + command.GetType().Name + " is not supported.", cancellationToken);
                                await binaryWriter.Write((string)null, cancellationToken);
                                await stream.FlushAsync(cancellationToken).ConfigureAwait(false);
                            }
                        }
                        finally
                        {
                            stream.Disconnect();
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                Console.ReadKey();
            }
        }
    }
}
