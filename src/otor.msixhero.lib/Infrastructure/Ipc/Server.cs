using System;
using System.Collections.Generic;
using System.IO.Pipes;
using System.Linq;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using otor.msixhero.lib.Domain.Commands;
using otor.msixhero.lib.Infrastructure.Ipc.Helpers;
using otor.msixhero.lib.Infrastructure.Ipc.Streams;
using otor.msixhero.lib.Infrastructure.Logging;
using otor.msixhero.lib.Infrastructure.Progress;

namespace otor.msixhero.lib.Infrastructure.Ipc
{
    public class Server
    {
        private static readonly ILog Logger = LogManager.GetLogger();

        private readonly object lockObject = new object();
        private readonly AutoResetEvent autoResetEvent = new AutoResetEvent(true);

        private readonly Dictionary<Type, Func<BaseCommand, CancellationToken, Progress.Progress, Task<string>>> handlersWithOutput = new Dictionary<Type, Func<BaseCommand, CancellationToken, Progress.Progress, Task<string>>>();
        private readonly Dictionary<Type, Func<BaseCommand, CancellationToken, Progress.Progress, Task>> handlersWithoutOutput = new Dictionary<Type, Func<BaseCommand, CancellationToken, Progress.Progress, Task>>();

        private readonly HashSet<Type> inputOutputTypes = new HashSet<Type>();

        public void AddHandler<TInput, TOutput>(Func<TInput, CancellationToken, Progress.Progress, Task<TOutput>> handler) where TInput : BaseCommand<TOutput>
        {
            Logger.Debug("Adding handler for {0} that returns the data {1}.", typeof(TInput).Name, typeof(TOutput).Name);
            // ReSharper disable once AssignNullToNotNullAttribute
            this.handlersWithOutput.Add(typeof(TInput), async (command, cancellationToken, progress) =>
            {
                var result = await handler((TInput) command, cancellationToken, progress).ConfigureAwait(false);
                return JsonConvert.SerializeObject(result, typeof(TOutput), new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.Auto });
            });

            this.inputOutputTypes.Add(typeof(TInput));
        }
        public void AddHandler<T>(Func<T, CancellationToken, Progress.Progress, Task> handler) where T : BaseCommand
        {
            Logger.Debug("Adding void handler for {0}.", typeof(T).Name);
            // ReSharper disable once AssignNullToNotNullAttribute
            this.handlersWithoutOutput.Add(typeof(T), (command, cancellationToken, progress) => handler((T)command, cancellationToken, progress));
            this.inputOutputTypes.Add(typeof(T));
        }

        public async Task Start(CancellationToken cancellationToken = default, Progress.Progress progress = default)
        {
            try
            {
                Logger.Debug("Creating named pipe security for {0}", WellKnownSidType.WorldSid);
                var ps = new PipeSecurity();
                var sid = new SecurityIdentifier(WellKnownSidType.WorldSid, null);
                Logger.Debug("Security SID is {0}", sid.Value);
                ps.SetAccessRule(new PipeAccessRule(sid, PipeAccessRights.ReadWrite, System.Security.AccessControl.AccessControlType.Allow));

                while (true)
                {
                    Logger.Debug("Creating named pipe {0}", "msixhero");
                    // ReSharper disable once StringLiteralTypo
                    using (var stream = NamedPipeNative.CreateNamedPipe("msixhero", ps))
                    {
                        Logger.Debug("Created stream " + stream.GetHashCode());

                        try
                        {
                            Logger.Debug("Waiting for a client connection...");
                            await stream.WaitForConnectionAsync(cancellationToken);

                            // using var stream = client.GetStream();
                            var binaryWriter = new BinaryStreamProcessor(stream);
                            var binaryReader = new BinaryStreamProcessor(stream);

                            Logger.Debug("Reading command from stream...");
                            var command = await binaryReader.Read<BaseCommand>(cancellationToken);

                            var isDone = false;
                            try
                            {
                                Func<BaseCommand, CancellationToken, Progress.Progress, Task> handlerWithoutOutput = null;

                                // ReSharper disable once AssignNullToNotNullAttribute
                                if (!this.handlersWithOutput.TryGetValue(command.GetType(), out var handlerWithOutput))
                                {
                                    if (!this.handlersWithoutOutput.TryGetValue(command.GetType(), out handlerWithoutOutput))
                                    {
                                        throw new NotSupportedException($"The command {command.GetType().FullName} is not supported. Only supported are:\r\n * {string.Join("\r\n * ", this.handlersWithOutput.Keys.Select(k => k))}");
                                    }
                                }


                                EventHandler<ProgressData> progressHandler = (sender, data) =>
                                {
                                    try
                                    {
                                        Logger.Trace("Beginning atomic scope with AutoResetEvent");
                                        this.autoResetEvent.WaitOne();
                                        if (!isDone)
                                        {
                                            Logger.Debug("Write progress data");
                                            binaryWriter.Write((int) ResponseType.Progress, cancellationToken).GetAwaiter().GetResult();

                                            Logger.Debug("{1}% {0}", data.Message, data.Progress);
                                            binaryWriter.Write(data, cancellationToken).GetAwaiter().GetResult();

                                            Logger.Trace("Flushing the stream...");
                                            stream.Flush();
                                        }
                                    }
                                    finally
                                    {
                                        Logger.Trace("Finishing atomic scope with AutoResetEvent");
                                        this.autoResetEvent.Set();
                                    }
                                };

                                if (progress != null)
                                {
                                    Logger.Debug("Subscribing to progress changed...");
                                    progress.ProgressChanged += progressHandler;
                                }

                                string result;
                                try
                                {
                                    if (handlerWithOutput != null)
                                    {
                                        Logger.Debug("Waiting for {0} to return results...", command.GetType().Name);
                                        result = await handlerWithOutput(command, cancellationToken, progress);
                                    }
                                    else if (handlerWithoutOutput != null)
                                    {
                                        Logger.Debug("Waiting for {0} to finish...", command.GetType().Name);
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
                                        Logger.Debug("Unsubscribing from progress changed...");
                                        progress.ProgressChanged -= progressHandler;
                                    }
                                }

                                try
                                {
                                    Logger.Trace("Beginning atomic scope with AutoResetEvent");
                                    this.autoResetEvent.WaitOne();

                                    isDone = true;

                                    Logger.Debug("Returning results via named pipe...");
                                    await binaryWriter.Write((int) ResponseType.Result, cancellationToken).ConfigureAwait(false);

                                    if (handlerWithOutput != null)
                                    {
                                        Logger.Debug("Returning actual results via named pipe...");
                                        await binaryWriter.Write(result, cancellationToken).ConfigureAwait(false);
                                    }

                                    Logger.Debug("Flushing the stream...");
                                    await stream.FlushAsync(cancellationToken).ConfigureAwait(false);

                                    Logger.Debug("Waiting for the pipe to drain...");
                                    stream.WaitForPipeDrain();
                                }
                                finally
                                {
                                    Logger.Trace("Finishing atomic scope with AutoResetEvent");
                                    this.autoResetEvent.Set();
                                }
                            }
                            catch (OperationCanceledException e)
                            {
                                Logger.Info(e);
                            }
                            catch (Exception e)
                            {
                                try
                                {
                                    Logger.Trace("Beginning atomic scope with AutoResetEvent");
                                    this.autoResetEvent.WaitOne();

                                    Logger.Error(e, "Reporting exception");
                                    Logger.Trace("Notifying about the response type ({0})...", ResponseType.Exception);
                                    await binaryWriter.Write((int)ResponseType.Exception, cancellationToken);
                                    Logger.Trace("Notifying about the message {0}...", e.Message);
                                    await binaryWriter.Write(e.Message, cancellationToken);
                                    Logger.Trace("Notifying about the callstack (0)...", e.ToString());
                                    await binaryWriter.Write(e.ToString(), cancellationToken);
                                    Logger.Trace("Flushing....");
                                    await stream.FlushAsync(cancellationToken).ConfigureAwait(false);
                                    Logger.Trace("Waiting for the pipe to drain....");
                                    stream.WaitForPipeDrain();
                                }
                                finally
                                {
                                    Logger.Trace("Finishing atomic scope with AutoResetEvent");
                                    this.autoResetEvent.Set();
                                }
                            }
                        }
                        catch (Exception e)
                        {
                            Logger.Error(e);
                        }
                        finally
                        {
                            if (stream.IsConnected)
                            {
                                Logger.Debug("Disconnecting the stream " + stream.GetHashCode());
                                stream.Disconnect();
                            }
                        }
                    }

                    Logger.Debug("Finishing the main loop, repeating now...");
                }
            }
            catch (Exception e)
            {
                Logger.Fatal(e);
            }

            Console.ReadKey();
        }
    }
}
