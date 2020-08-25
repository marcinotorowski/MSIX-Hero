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
