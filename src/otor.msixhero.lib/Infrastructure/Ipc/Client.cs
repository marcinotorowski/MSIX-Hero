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
            var process = Process.GetProcessesByName("otor.msixhero.adminhelper").FirstOrDefault();

            if (process == null || process.HasExited)
            {
                // make double checking with help of mutex
                if (!SafeHandle.WaitOne(TimeSpan.FromSeconds(30)))
                {
                    throw new InvalidOperationException("Could not get exclusive access.");
                }

                try
                {
                    process = Process.GetProcessesByName("otor.msixhero.adminhelper").FirstOrDefault();
                    if (process == null || process.HasExited)
                    {
                        var psi = new ProcessStartInfo(string.Join(AppDomain.CurrentDomain.BaseDirectory, "otor.msixhero.adminhelper.exe"), "--selfElevate")
                        {
                            Verb = "runas",
                            UseShellExecute = true,
                            // WindowStyle = ProcessWindowStyle.Hidden,
                            // CreateNoWindow = true
                        };

                        var p = processManager.Start(psi);
                        if (p == null)
                        {
                            throw new InvalidOperationException("Could not start the helper.");
                        }

                        await Task.Delay(400, cancellationToken).ConfigureAwait(false);
                    }
                }
                finally
                {
                    SafeHandle.Set();
                }
            }

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
            var process = Process.GetProcessesByName("otor.msixhero.adminhelper").FirstOrDefault();
            
            if (process == null || process.HasExited)
            {
                process = Process.GetProcessesByName("otor.msixhero.adminhelper").FirstOrDefault();
                if (process == null || process.HasExited)
                {
                    var psi = new ProcessStartInfo(string.Join(AppDomain.CurrentDomain.BaseDirectory, "otor.msixhero.adminhelper.exe"), "--selfElevate")
                    {
                        Verb = "runas",
                        UseShellExecute = true,
#if !DEBUG
                WindowStyle = ProcessWindowStyle.Hidden,
                CreateNoWindow = true
#endif
                    };

                    var p = processManager.Start(psi);
                    if (p == null)
                    {
                        throw new InvalidOperationException("Could not start the helper.");
                    }
                }
            }

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
