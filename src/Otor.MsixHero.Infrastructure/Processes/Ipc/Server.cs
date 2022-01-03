// MSIX Hero
// Copyright (C) 2022 Marcin Otorowski
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// Full notice:
// https://github.com/marcinotorowski/msix-hero/blob/develop/LICENSE.md

using System;
using System.Collections.Generic;
using System.IO.Pipes;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Otor.MsixHero.Infrastructure.Configuration.ResolvableFolder;
using Otor.MsixHero.Infrastructure.Helpers;
using Otor.MsixHero.Infrastructure.Helpers.Streams;
using Otor.MsixHero.Infrastructure.Logging;
using Otor.MsixHero.Infrastructure.Processes.SelfElevation.Enums;
using Otor.MsixHero.Infrastructure.Progress;

namespace Otor.MsixHero.Infrastructure.Processes.Ipc
{
    public class Server
    {
        private readonly IDictionary<Type, ISelfElevationProxyReceiver> receivers;

        private static readonly JsonSerializerSettings SerializerSettings = new JsonSerializerSettings
        {
            TypeNameHandling = TypeNameHandling.Auto,
            Converters = new List<JsonConverter>
            {
                new ResolvablePathConverter()
            }
        };

        private static readonly ILog Logger = LogManager.GetLogger();
        private readonly AutoResetEvent autoResetEvent = new AutoResetEvent(true);

        public Server(IEnumerable<ISelfElevationProxyReceiver> receivers)
        {
            this.receivers = new Dictionary<Type, ISelfElevationProxyReceiver>();

            foreach (var r in receivers)
            {
                foreach (var t in r.GetSupportedProxiedObjectTypes())
                {
                    this.receivers[t] = r;
                }
            }
        }

        public async Task Start(int maxRequests = 0, CancellationToken cancellationToken = default)
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
                            await stream.WaitForConnectionAsync(cancellationToken).ConfigureAwait(false);

                            // using var stream = client.GetStream();
                            var binaryWriter = new BinaryStreamProcessor(stream);
                            var binaryReader = new BinaryStreamProcessor(stream);

                            Logger.Debug("Reading command from stream...");
                            var command = await binaryReader.Read<IProxyObject>(cancellationToken).ConfigureAwait(false);

                            var commandType = command.GetType();
                            if (!this.receivers.TryGetValue(commandType, out var receiver))
                            {
                                throw new NotSupportedException("No receiver for type " + commandType.Name + " registered.");
                            }
                            
                            string result = null;

                            var progress = new Progress<ProgressData>();
                            EventHandler<ProgressData> progressChanged = async (sender, data) =>
                            {
                                try
                                {
                                    this.autoResetEvent.WaitOne();
                                    await binaryWriter.Write((int)ResponseType.Progress, cancellationToken).ConfigureAwait(false);
                                    await binaryReader.Write(data, cancellationToken).ConfigureAwait(false);
                                }
                                finally
                                {
                                    this.autoResetEvent.Set();
                                }
                            };

                            try
                            {
                                progress.ProgressChanged += progressChanged;
                                var returnedType = GenericArgumentResolver.GetResultType(commandType, typeof(IProxyObjectWithOutput<>));

                                if (returnedType == null)
                                {
                                    Console.WriteLine($"Requested: {commandType.Name} with no returned value...");
                                    await receiver.Invoke(command, cancellationToken, progress).ConfigureAwait(false);
                                }
                                else
                                {
                                    Console.WriteLine($"Requested: {commandType.Name} with return value of type {returnedType.Name}...");
                                    var executionResult = await receiver.Get((IProxyObjectWithOutput<object>)command, cancellationToken, progress).ConfigureAwait(false);
                                    result = JsonConvert.SerializeObject(executionResult, Formatting.None, SerializerSettings);
                                }

                                try
                                {
                                    Logger.Trace("Beginning atomic scope with AutoResetEvent");
                                    this.autoResetEvent.WaitOne();

                                    Logger.Debug("Returning results via named pipe...");
                                    await binaryWriter.Write((int)ResponseType.Result, cancellationToken).ConfigureAwait(false);

                                    if (returnedType != null)
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
                            finally
                            {
                                progress.ProgressChanged -= progressChanged;
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
