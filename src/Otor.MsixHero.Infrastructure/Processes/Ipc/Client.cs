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
using System.Threading;
using System.Threading.Tasks;
using Otor.MsixHero.Infrastructure.Helpers.Streams;
using Otor.MsixHero.Infrastructure.Progress;

namespace Otor.MsixHero.Infrastructure.Processes.Ipc
{
    public class Client : IElevatedClient
    {
        private readonly IInterProcessCommunicationManager ipcManager;
        
        public Client(IInterProcessCommunicationManager ipcManager)
        {
            this.ipcManager = ipcManager;
        }

        public Task<bool> Test(CancellationToken cancellationToken = default)
        {
            return this.ipcManager.Test(cancellationToken);
        }

        public async Task Invoke(IProxyObject command, CancellationToken cancellationToken = default, IProgress<ProgressData> progress = default)
        {
            // ReSharper disable once StringLiteralTypo
            using (var pipeClient = await this.ipcManager.GetCommunicationChannel(cancellationToken).ConfigureAwait(false))
            {
                await pipeClient.ConnectAsync(4000, cancellationToken).ConfigureAwait(false);
                var stream = pipeClient;

                var binaryProcessor = new BinaryStreamProcessor(stream);
                // ReSharper disable once RedundantCast
                await binaryProcessor.Write((IProxyObject)command, cancellationToken).ConfigureAwait(false);
                await stream.FlushAsync(cancellationToken).ConfigureAwait(false);

                while (true)
                {
                    var response = (ResponseType)await binaryProcessor.ReadInt32(cancellationToken).ConfigureAwait(false);
                    
                    switch (response)
                    {
                        case ResponseType.Exception:
                            var msg = await binaryProcessor.ReadString(cancellationToken).ConfigureAwait(false);
                            // ReSharper disable once UnusedVariable
                            var stack = await binaryProcessor.ReadString(cancellationToken).ConfigureAwait(false);
                            await stream.FlushAsync(cancellationToken).ConfigureAwait(false);
                            throw new ForwardedException(msg);

                        case ResponseType.Progress:
                            var deserializedProgress = await binaryProcessor.Read<ProgressData>(cancellationToken).ConfigureAwait(false);
                            if (progress != null)
                            {
                                progress.Report(deserializedProgress);
                            }

                            break;

                        case ResponseType.Result:
                            if (progress != null)
                            {
                                progress.Report(new ProgressData(100, null));
                            }

                            return;

                        default:
                            throw new NotSupportedException();
                    }
                }
            }
        }

        // ReSharper disable once MemberCanBeMadeStatic.Global
        public async Task<TOutput> Get<TOutput>(IProxyObjectWithOutput<TOutput> command, CancellationToken cancellationToken = default, IProgress<ProgressData> progress = default)
        {
            // ReSharper disable once StringLiteralTypo
            using (var pipeClient = await this.ipcManager.GetCommunicationChannel(cancellationToken).ConfigureAwait(false))
            {
                await pipeClient.ConnectAsync(4000, cancellationToken).ConfigureAwait(false);
                var stream = pipeClient;

                var binaryProcessor = new BinaryStreamProcessor(stream);
                await binaryProcessor.Write((IProxyObject)command, cancellationToken).ConfigureAwait(false);
                await stream.FlushAsync(cancellationToken).ConfigureAwait(false);
                
                while (true)
                {
                    var response = (ResponseType) await binaryProcessor.ReadInt32(cancellationToken).ConfigureAwait(false);

                    switch (response)
                    {
                        case ResponseType.Exception:
                            var msg = await binaryProcessor.ReadString(cancellationToken).ConfigureAwait(false);
                            // ReSharper disable once UnusedVariable
                            var stack = await binaryProcessor.ReadString(cancellationToken).ConfigureAwait(false);
                            await stream.FlushAsync(cancellationToken).ConfigureAwait(false);
                            throw new ForwardedException(msg);

                        case ResponseType.Result:
                            var deserializedObject = await binaryProcessor.Read<TOutput>(cancellationToken).ConfigureAwait(false);
                            if (progress != null)
                            {
                                progress.Report(new ProgressData(100, null));
                            }

                            return deserializedObject;

                        case ResponseType.Progress:
                            var deserializedProgress = await binaryProcessor.Read<ProgressData>(cancellationToken).ConfigureAwait(false);
                            if (progress != null)
                            {
                                progress.Report(deserializedProgress);
                            }

                            break;

                        default:
                            throw new NotSupportedException();
                    }
                }
            }
        }
    }
}
