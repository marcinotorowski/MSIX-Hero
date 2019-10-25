using System;
using System.Diagnostics;
using System.IO.Pipes;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using otor.msixhero.lib.BusinessLayer.Actions;
using otor.msixhero.lib.Ipc.Streams;

namespace otor.msixhero.lib.Ipc
{
    public class Client
    {   
        // ReSharper disable once MemberCanBeMadeStatic.Global
        public async Task<TOutput> Execute<TInput, TOutput>(TInput command, CancellationToken cancellationToken) where TInput : BaseAction
        {
            var processPid = Process.GetCurrentProcess().Id;
            var process = processPid == 0 ? null : Process.GetProcessesByName("otor.msixhero.adminhelper").FirstOrDefault(p => p.Id == processPid);
            
            if (process == null || process.HasExited)
            {
                var psi = new ProcessStartInfo(string.Join(AppDomain.CurrentDomain.BaseDirectory, "otor.msixhero.adminhelper.exe"), "--pipe " + Process.GetCurrentProcess().Id)
                {
                    Verb = "runas",
                    UseShellExecute = true,
                    WindowStyle = ProcessWindowStyle.Hidden,
                    CreateNoWindow = true
                };

                var p = Process.Start(psi);
                if (p == null)
                {
                    throw new InvalidOperationException("Could not start the helper.");
                }

                await Task.Delay(400, cancellationToken).ConfigureAwait(false);
            }

            await using var pipeClient = new NamedPipeClientStream("msixhero-" + Process.GetCurrentProcess().Id);
            await pipeClient.ConnectAsync(90000, cancellationToken).ConfigureAwait(false);
            var stream = pipeClient;

            var binaryProcessor = new BinaryStreamProcessor(stream);
            await binaryProcessor.Write((BaseAction)command, cancellationToken);
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
