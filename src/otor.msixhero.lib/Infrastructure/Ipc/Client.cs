using System;
using System.Diagnostics;
using System.IO.Pipes;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using otor.msixhero.lib.Domain.Commands;
using otor.msixhero.lib.Infrastructure.Interop;
using otor.msixhero.lib.Infrastructure.Ipc.Streams;
using otor.msixhero.lib.Infrastructure.Progress;

namespace otor.msixhero.lib.Infrastructure.Ipc
{
    public class Client
    {
        private static readonly AutoResetEvent SafeHandle = new AutoResetEvent(true);

        private readonly IProcessManager processManager;
        
        public Client(IProcessManager processManager)
        {
            this.processManager = processManager;
        }

        public async Task Execute(BaseCommand command, CancellationToken cancellationToken = default, IProgress<ProgressData> progress = default)
        {
            await this.processManager.Connect(cancellationToken).ConfigureAwait(false);

            // ReSharper disable once StringLiteralTypo
            await using var pipeClient = new NamedPipeClientStream("msixhero");
            await pipeClient.ConnectAsync(4000, cancellationToken).ConfigureAwait(false);
            var stream = pipeClient;

            var binaryProcessor = new BinaryStreamProcessor(stream);
            // ReSharper disable once RedundantCast
            await binaryProcessor.Write((BaseCommand)command, cancellationToken);
            await stream.FlushAsync(cancellationToken).ConfigureAwait(false);

            while (true)
            {
                var response = (ResponseType)await binaryProcessor.ReadInt32(cancellationToken).ConfigureAwait(false);
                
                switch (response)
                {
                    case ResponseType.Exception:
                        var msg = await binaryProcessor.ReadString(cancellationToken).ConfigureAwait(false);
                        var stack = await binaryProcessor.ReadString(cancellationToken).ConfigureAwait(false);
                        await stream.FlushAsync(cancellationToken).ConfigureAwait(false);
                        throw new Exception(msg);

                    case ResponseType.Progress:
                        var deserializedProgress = await binaryProcessor.Read<ProgressData>(cancellationToken).ConfigureAwait(false);
                        if (progress != null)
                        {
                            progress.Report(deserializedProgress);
                        }

                        break;

                    case ResponseType.Result:
                        return;

                    default:
                        throw new NotSupportedException();
                }
            }
        }

        // ReSharper disable once MemberCanBeMadeStatic.Global
        public async Task<TOutput> GetExecuted<TOutput>(BaseCommand<TOutput> command, CancellationToken cancellationToken = default, IProgress<ProgressData> progress = default)
        {
            await this.processManager.Connect(cancellationToken).ConfigureAwait(false);

            // ReSharper disable once StringLiteralTypo
            await using var pipeClient = new NamedPipeClientStream("msixhero");
            await pipeClient.ConnectAsync(4000, cancellationToken).ConfigureAwait(false);
            var stream = pipeClient;

            var binaryProcessor = new BinaryStreamProcessor(stream);
            await binaryProcessor.Write((BaseCommand)command, cancellationToken);
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
                        throw new Exception(msg);

                    case ResponseType.Result:
                        var deserializedObject = await binaryProcessor.Read<TOutput>(cancellationToken).ConfigureAwait(false);
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
