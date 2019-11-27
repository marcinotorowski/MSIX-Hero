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

        private readonly Dictionary<Type, Func<BaseCommand, CancellationToken, Progress, Task<byte[]>>> handlersWithOutput = new Dictionary<Type, Func<BaseCommand, CancellationToken, Progress, Task<byte[]>>>();
        private readonly Dictionary<Type, Func<BaseCommand, CancellationToken, Progress, Task>> handlersWithoutOutput = new Dictionary<Type, Func<BaseCommand, CancellationToken, Progress, Task>>();

        private readonly HashSet<Type> inputOutputTypes = new HashSet<Type>();

        public void AddHandler<T>(Func<T, CancellationToken, Progress, Task<byte[]>> handler) where T : BaseCommand
        {
            // ReSharper disable once AssignNullToNotNullAttribute
            this.handlersWithOutput.Add(typeof(T), (command, cancellationToken, progress) => handler((T)command, cancellationToken, progress));
            this.inputOutputTypes.Add(typeof(T));
        }
        public void AddHandler<T>(Func<T, CancellationToken, Progress, Task> handler) where T : BaseCommand
        {
            // ReSharper disable once AssignNullToNotNullAttribute
            this.handlersWithoutOutput.Add(typeof(T), (command, cancellationToken, progress) => handler((T)command, cancellationToken, progress));
            this.inputOutputTypes.Add(typeof(T));
        }

        public async Task Start(CancellationToken cancellationToken = default, Progress progress = default)
        {
            try
            {
                var ps = new PipeSecurity();
                var sid = new SecurityIdentifier(WellKnownSidType.WorldSid, null);
                ps.SetAccessRule(new PipeAccessRule(sid, PipeAccessRights.ReadWrite, System.Security.AccessControl.AccessControlType.Allow));

                while (true)
                {
                    Console.WriteLine("Waiting for a client connection...");

                    // ReSharper disable once StringLiteralTypo
                    using (var stream = NamedPipeNative.CreateNamedPipe("msixhero", ps))
                    {
                        try
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
                                Func<BaseCommand, CancellationToken, Progress, Task> handlerWithoutOutput = null;

                                // ReSharper disable once AssignNullToNotNullAttribute
                                if (!this.handlersWithOutput.TryGetValue(command.GetType(), out var handlerWithOutput))
                                {
                                    if (!this.handlersWithoutOutput.TryGetValue(command.GetType(),
                                        out handlerWithoutOutput))
                                    {
                                        throw new NotSupportedException(
                                            $"The command {command.GetType().FullName} is not supported. Only supported are:\r\n * {string.Join("\r\n * ", this.handlersWithOutput.Keys.Select(k => k))}");
                                    }
                                }


                                EventHandler<ProgressData> progressHandler = (sender, data) =>
                                {
                                    try
                                    {
                                        this.autoResetEvent.WaitOne();
                                        if (!isDone)
                                        {
                                            Console.WriteLine("Write " + (int) ResponseType.Progress);
                                            binaryWriter.Write((int) ResponseType.Progress, cancellationToken)
                                                .GetAwaiter()
                                                .GetResult();
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
                                    if (handlerWithOutput != null)
                                    {
                                        result = await handlerWithOutput(command, cancellationToken, progress);
                                    }
                                    else if (handlerWithoutOutput != null)
                                    {
                                        await handlerWithoutOutput(command, cancellationToken, progress);
                                        result = null;
                                    }
                                    else
                                    {
                                        throw new InvalidOperationException();
                                    }
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
                                    Console.WriteLine("Return " + ResponseType.Result);
                                    await binaryWriter.Write((int) ResponseType.Result, cancellationToken)
                                        .ConfigureAwait(false);

                                    if (handlerWithOutput != null)
                                    {
                                        Console.WriteLine("Write results");
                                        await binaryWriter.Write(result, cancellationToken).ConfigureAwait(false);
                                    }

                                    await stream.FlushAsync(cancellationToken).ConfigureAwait(false);
                                    stream.WaitForPipeDrain();
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
                                    Console.WriteLine("Return " + ResponseType.Exception + ": " + e.GetType().Name);
                                    await binaryWriter.Write((int) ResponseType.Exception, cancellationToken);
                                    Console.WriteLine(e);
                                    await binaryWriter.Write(e.Message, cancellationToken);
                                    await binaryWriter.Write(e.ToString(), cancellationToken);
                                    await stream.FlushAsync(cancellationToken).ConfigureAwait(false);
                                    stream.WaitForPipeDrain();
                                }
                                finally
                                {
                                    this.autoResetEvent.Set();
                                }
                            }
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e);
                        }
                        finally
                        {
                            if (stream.IsConnected)
                            {
                                stream.Disconnect();
                            }
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
