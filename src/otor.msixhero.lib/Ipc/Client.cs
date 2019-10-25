using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Serialization;
using otor.msixhero.lib.Ipc.Commands;
using otor.msixhero.lib.Ipc.Streams;

namespace otor.msixhero.lib.Ipc
{
    public interface IClientCommandExecutor
    {
        Task<TOutput> Execute<TOutput>(BaseCommand command);
    }

    public class Client
    {   
        public async Task<T> Execute<T>(T command) where T : BaseCommand
        {
            var processPid = System.Diagnostics.Process.GetCurrentProcess().Id;
            var process = processPid == 0 ? null : Process.GetProcessesByName("otor.msixhero.adminhelper").FirstOrDefault(p => p.Id == processPid);
            
            if (process == null || process.HasExited)
            {
                var psi = new ProcessStartInfo(string.Join(AppDomain.CurrentDomain.BaseDirectory, "otor.msixhero.adminhelper" + ".exe"), "--pipe " + Process.GetCurrentProcess().Id);
                psi.Verb = "runas";
                psi.UseShellExecute = true;
                psi.WindowStyle = ProcessWindowStyle.Hidden;
                psi.CreateNoWindow = true;

                var p = Process.Start(psi);
                await Task.Delay(400).ConfigureAwait(false);
                processPid = p.Id;
            }

            using var pipeClient = new NamedPipeClientStream("msixhero-" + Process.GetCurrentProcess().Id);
            await pipeClient.ConnectAsync(90000).ConfigureAwait(false);
            var stream = pipeClient;

            var binaryProcessor = new BinaryStreamProcessor(stream);
            await binaryProcessor.Write((BaseCommand)command);
            await stream.FlushAsync().ConfigureAwait(false);
            var success = await binaryProcessor.ReadBoolean().ConfigureAwait(false);

            if (!success)
            {
                var msg = await binaryProcessor.ReadString().ConfigureAwait(false);
                // ReSharper disable once UnusedVariable
                var stack = await binaryProcessor.ReadString().ConfigureAwait(false);
                await binaryProcessor.Write(true).ConfigureAwait(false);
                await stream.FlushAsync().ConfigureAwait(false);

                throw new Exception(msg);
            }

            var deserializedObject = await binaryProcessor.Read<BaseCommand>().ConfigureAwait(false);
            await binaryProcessor.Write(true).ConfigureAwait(false);
            await stream.FlushAsync().ConfigureAwait(false);
            return (T)deserializedObject;
        }
    }
}
