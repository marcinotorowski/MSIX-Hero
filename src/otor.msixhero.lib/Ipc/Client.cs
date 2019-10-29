using System;
using System.Diagnostics;
using System.IO.Pipes;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using otor.msixhero.lib.BusinessLayer.Commands;
using otor.msixhero.lib.BusinessLayer.Infrastructure.Implementation;
using otor.msixhero.lib.Ipc.Streams;

namespace otor.msixhero.lib.Ipc
{
    public class Client
    {
        private static readonly AutoResetEvent SafeHandle = new AutoResetEvent(true);

        private readonly IProcessManager processManager;
        
        public Client(IProcessManager processManager)
        {
            this.processManager = processManager;
        }

        public async Task Execute<TInput>(TInput command, CancellationToken cancellationToken) where TInput : BaseCommand
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
            await pipeClient.ConnectAsync(90000, cancellationToken).ConfigureAwait(false);
            var stream = pipeClient;

            var binaryProcessor = new BinaryStreamProcessor(stream);
            await binaryProcessor.Write((BaseCommand)command, cancellationToken);
            await stream.FlushAsync(cancellationToken).ConfigureAwait(false);
            var success = await binaryProcessor.ReadBoolean(cancellationToken).ConfigureAwait(false);

            if (!success)
            {
                var msg = await binaryProcessor.ReadString(cancellationToken).ConfigureAwait(false);
                // ReSharper disable once UnusedVariable
                var stack = await binaryProcessor.ReadString(cancellationToken).ConfigureAwait(false);
                await stream.FlushAsync(cancellationToken).ConfigureAwait(false);
                throw new Exception(msg);
            }
        }

        // ReSharper disable once MemberCanBeMadeStatic.Global
        public async Task<TOutput> Execute<TInput, TOutput>(TInput command, CancellationToken cancellationToken) where TInput : BaseCommand
        {
            var process = Process.GetProcessesByName("otor.msixhero.adminhelper").FirstOrDefault();
            
            if (process == null || process.HasExited)
            {
                // make double checking with help of mutex
                // if (!ClientMutex.WaitOne(TimeSpan.FromSeconds(30)))
                // {
                //     throw new InvalidOperationException("Could not get exclusive access.");
                // }

                try
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

                        await Task.Delay(400, cancellationToken).ConfigureAwait(false);
                    }
                }
                finally
                {
                    //ClientMutex.ReleaseMutex();
                }
            }

            // ReSharper disable once StringLiteralTypo
            await using var pipeClient = new NamedPipeClientStream("msixhero");
            await pipeClient.ConnectAsync(90000, cancellationToken).ConfigureAwait(false);
            var stream = pipeClient;

            var binaryProcessor = new BinaryStreamProcessor(stream);
            await binaryProcessor.Write((BaseCommand)command, cancellationToken);
            await stream.FlushAsync(cancellationToken).ConfigureAwait(false);
            var success = await binaryProcessor.ReadBoolean(cancellationToken).ConfigureAwait(false);

            if (!success)
            {
                var msg = await binaryProcessor.ReadString(cancellationToken).ConfigureAwait(false);
                // ReSharper disable once UnusedVariable
                var stack = await binaryProcessor.ReadString(cancellationToken).ConfigureAwait(false);
                await stream.FlushAsync(cancellationToken).ConfigureAwait(false);
                throw new Exception(msg);
            }

            var deserializedObject = await binaryProcessor.Read<TOutput>(cancellationToken).ConfigureAwait(false);
            return deserializedObject;
        }
    }
}
