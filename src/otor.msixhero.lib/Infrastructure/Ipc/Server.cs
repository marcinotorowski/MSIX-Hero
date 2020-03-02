using System;
using System.Collections.Generic;
using System.IO.Pipes;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using otor.msixhero.lib.BusinessLayer.State;
using otor.msixhero.lib.Domain.Commands;
using otor.msixhero.lib.Infrastructure.Configuration.ResolvableFolder;
using otor.msixhero.lib.Infrastructure.Helpers;
using otor.msixhero.lib.Infrastructure.Ipc.Helpers;
using otor.msixhero.lib.Infrastructure.Ipc.Streams;
using otor.msixhero.lib.Infrastructure.Logging;
using otor.msixhero.lib.Infrastructure.Progress;

namespace otor.msixhero.lib.Infrastructure.Ipc
{
    public class Server
    {
        private static readonly JsonSerializerSettings SerializerSettings = new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.Auto,
            Converters = new List<JsonConverter>
            {
                new ResolvablePathConverter()
            }
        };

        private readonly IApplicationStateManager applicationStateManager;
        private static readonly ILog Logger = LogManager.GetLogger();
        private readonly AutoResetEvent autoResetEvent = new AutoResetEvent(true);
        
        public Server(IApplicationStateManager applicationStateManager)
        {
            this.applicationStateManager = applicationStateManager;
        }

        public async Task Start(int maxRequests = 0, CancellationToken cancellationToken = default, IBusyManager busyManager= default)
        {
            try
            {
                Logger.Debug("Creating named pipe security for {0}", WellKnownSidType.WorldSid);
                var ps = new PipeSecurity();
                var sid = new SecurityIdentifier(WellKnownSidType.WorldSid, null);
                Logger.Debug("Security SID is {0}", sid.Value);
                ps.SetAccessRule(new PipeAccessRule(sid, PipeAccessRights.ReadWrite, System.Security.AccessControl.AccessControlType.Allow));

                var cnt = 0;
                while (maxRequests <= 0 || cnt < maxRequests)
                {
                    if (maxRequests > 0)
                    {
                        cnt++;
                    }

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

                            var success = false;
                            var isDone = false;
                            try
                            {
                                EventHandler<IBusyStatusChange> progressHandler = (sender, data) =>
                                {
                                    try
                                    {
                                        Logger.Trace("Beginning atomic scope with AutoResetEvent");
                                        this.autoResetEvent.WaitOne();
                                        // ReSharper disable once AccessToModifiedClosure
                                        if (!isDone)
                                        {
                                            Logger.Debug("Write progress data");
                                            binaryWriter.Write((int) ResponseType.Progress, cancellationToken).GetAwaiter().GetResult();

                                            Logger.Debug("{1}% {0}", data.Message, data.Progress);
                                            binaryWriter.Write(new ProgressData(data.Progress, data.Message), cancellationToken).GetAwaiter().GetResult();

                                            Logger.Trace("Flushing the stream...");
                                            // ReSharper disable once AccessToDisposedClosure
                                            stream.Flush();
                                        }
                                    }
                                    finally
                                    {
                                        Logger.Trace("Finishing atomic scope with AutoResetEvent");
                                        this.autoResetEvent.Set();
                                    }
                                };

                                if (busyManager != null)
                                {
                                    Logger.Debug("Subscribing to progress changed...");
                                    busyManager.StatusChanged += progressHandler;
                                }

                                string result;
                                bool returnsValue;

                                try
                                {
                                    var commandType = command.GetType();
                                    Console.WriteLine($"Requested: {commandType.Name}...");
                                    var returnedType = GenericArgumentResolver.GetResultType(commandType, typeof(BaseCommand<>));

                                    returnsValue = returnedType != null;
                                    
                                    if (!returnsValue)
                                    {
                                        var p = busyManager?.Begin();
                                        try
                                        {
                                            await this.applicationStateManager.CommandExecutor.ExecuteAsync(command, cancellationToken, p).ConfigureAwait(false);
                                            result = null;
                                            success = true;
                                        }
                                        finally
                                        {
                                            if (busyManager != null && p != null)
                                            {
                                                busyManager.End(p);
                                            }
                                        }
                                    }
                                    else
                                    {
                                        var p = busyManager?.Begin();
                                        try
                                        {
                                            var executionResult = await this.applicationStateManager.CommandExecutor.GetExecuteAsync(command, cancellationToken, p).ConfigureAwait(false);
                                            result = JsonConvert.SerializeObject(executionResult, Formatting.None, SerializerSettings);
                                            success = true;
                                        }
                                        finally
                                        {
                                            if (busyManager != null && p != null)
                                            {
                                                busyManager.End(p);
                                            }
                                        }
                                    }
                                }
                                finally
                                {
                                    if (busyManager != null)
                                    {
                                        Logger.Debug("Unsubscribing from progress changed...");
                                        busyManager.StatusChanged -= progressHandler;
                                    }
                                }

                                if (success)
                                {
                                    try
                                    {
                                        Logger.Trace("Beginning atomic scope with AutoResetEvent");
                                        this.autoResetEvent.WaitOne();

                                        isDone = true;

                                        Logger.Debug("Returning results via named pipe...");
                                        await binaryWriter.Write((int)ResponseType.Result, cancellationToken).ConfigureAwait(false);

                                        if (returnsValue)
                                        {
                                            Console.WriteLine("Sending the results...");
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
                                    await binaryWriter.Write((int)ResponseType.Exception, cancellationToken).ConfigureAwait(false);
                                    Logger.Trace("Notifying about the message {0}...", e.Message);
                                    await binaryWriter.Write(e.Message, cancellationToken).ConfigureAwait(false);
                                    Logger.Trace("Notifying about the callstack (0)...", e);
                                    await binaryWriter.Write(e.ToString(), cancellationToken).ConfigureAwait(false);
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
        }
    }
}
