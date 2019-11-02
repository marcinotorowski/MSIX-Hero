using System;
using System.Collections.Generic;
using System.IO.Pipes;
using System.Linq;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;
using otor.msixhero.lib.BusinessLayer.Commands;
using otor.msixhero.lib.Domain;
using otor.msixhero.lib.Ipc.Helpers;
using otor.msixhero.lib.Ipc.Streams;

namespace otor.msixhero.lib.Ipc
{
    public class Server
    {
        private readonly object lockObject = new object();
        private readonly AutoResetEvent autoResetEvent = new AutoResetEvent(true);

        private readonly Dictionary<Type, Func<BaseCommand, CancellationToken, Progress, Task<byte[]>>> handlers = new Dictionary<Type, Func<BaseCommand, CancellationToken, Progress, Task<byte[]>>>();
        private readonly HashSet<Type> inputOutputTypes = new HashSet<Type>();

        public void AddHandler<T>(Func<T, CancellationToken, Progress, Task<byte[]>> handler) where T : BaseCommand
        {
            // ReSharper disable once AssignNullToNotNullAttribute
            this.handlers.Add(typeof(T), (command, cancellationToken, progress) => handler((T)command, cancellationToken, progress));
            this.inputOutputTypes.Add(typeof(T));
        }

        public async Task Start(CancellationToken cancellationToken = default, Progress progress = default)
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

                        var isDone = false;
                        try
                        {
                            try
                            {
                                // ReSharper disable once AssignNullToNotNullAttribute
                                if (!this.handlers.TryGetValue(command.GetType(), out var handler))
                                {
                                    throw new NotSupportedException("The command " + command.GetType().FullName + " is not supported. Only supported are:\r\n * " + string.Join("\r\n * ", this.handlers.Keys.Select(k => k)));
                                }


                                EventHandler<ProgressData> progressHandler = (sender, data) =>
                                {
                                    try
                                    {
                                        this.autoResetEvent.WaitOne();
                                        if (!isDone)
                                        {
                                            Console.WriteLine("Write " + (int)ResponseType.Progress);
                                            binaryWriter.Write((int)ResponseType.Progress, cancellationToken).GetAwaiter().GetResult();
                                            Console.WriteLine("Write progress data");
                                            binaryWriter.Write(data, cancellationToken).GetAwaiter().GetResult();
                                            stream.Flush();
                                        }
                                    }
                                    finally
                                    {
                                        this.autoResetEvent.Set();
                                    }
                                };

                                if (progress != null)
                                {
                                    progress.ProgressChanged += progressHandler;
                                }

                                byte[] result;
                                try
                                {
                                    result = await handler(command, cancellationToken, progress);
                                }
                                finally
                                {
                                    if (progress != null)
                                    {
                                        progress.ProgressChanged -= progressHandler;
                                    }
                                }

                                try
                                {
                                    this.autoResetEvent.WaitOne();

                                    isDone = true;
                                    Console.WriteLine("Write " + (int)ResponseType.Progress);
                                    await binaryWriter.Write((int) ResponseType.Result, cancellationToken).ConfigureAwait(false);
                                    Console.WriteLine("Write results");
                                    await binaryWriter.Write(result, cancellationToken).ConfigureAwait(false);
                                    await stream.FlushAsync(cancellationToken).ConfigureAwait(false);
                                }
                                finally
                                {
                                    this.autoResetEvent.Set();
                                }
                            }
                            catch (OperationCanceledException)
                            {
                            }
                            catch (Exception e)
                            {
                                try
                                {
                                    this.autoResetEvent.WaitOne();
                                    Console.WriteLine("Write " + (int)ResponseType.Progress);
                                    await binaryWriter.Write((int)ResponseType.Exception, cancellationToken); 
                                    Console.WriteLine("Write exception");
                                    await binaryWriter.Write(e.Message, cancellationToken);
                                    await binaryWriter.Write(e.ToString(), cancellationToken);
                                    await stream.FlushAsync(cancellationToken).ConfigureAwait(false);
                                }
                                finally
                                {
                                    this.autoResetEvent.Set();
                                }
                            }

                            // end of protocol
                            await binaryReader.ReadBoolean(cancellationToken).ConfigureAwait(false);
                            
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
