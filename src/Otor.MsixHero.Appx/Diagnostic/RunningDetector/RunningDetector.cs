using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using Otor.MsixHero.Appx.Packaging.Installation.Entities;
using Timer = System.Timers.Timer;

namespace Otor.MsixHero.Appx.Diagnostic.RunningDetector
{
    public class RunningDetector : IRunningDetector, IDisposable
    {
        private readonly IList<Subscriber> observers = new List<Subscriber>();

        private readonly Timer timer = new Timer(2000);
        private IDictionary<string, string> consideredPackages;
        private HashSet<string> previouslyRunningAppIds;

        public IDisposable Subscribe(IObserver<ActivePackageFullNames> observer)
        {
            var returned = new Subscriber(this, observer);
            observers.Add(returned);
            return returned;
        }

        public async Task Listen(IList<InstalledPackage> installedPackages, CancellationToken cancellationToken)
        {
            var path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "WindowsApps") + "\\";
            this.consideredPackages = new Dictionary<string, string>();

            foreach (var item in installedPackages.Where(p => p.ManifestLocation.StartsWith(path, StringComparison.OrdinalIgnoreCase)))
            {
                this.consideredPackages[item.ManifestLocation.Substring(0, item.ManifestLocation.IndexOf('\\', path.Length + 1))] = item.PackageId;
            }

            this.timer.Elapsed -= this.TimerOnElapsed;
            this.timer.Elapsed += this.TimerOnElapsed;
            this.timer.Start();

            await this.DoCheck().ConfigureAwait(false);
        }

        private void TimerOnElapsed(object sender, ElapsedEventArgs e)
        {
            this.DoCheck();
        }

        private async Task DoCheck()
        {
            if (!this.timer.Enabled || this.consideredPackages == null)
            {
                return;
            }

            var folder = Environment.GetFolderPath(Environment.SpecialFolder.System);
            var file = Path.Combine(folder, "tasklist.exe");
            var cmd = "/apps /fi \"status eq running\" /fo CSV";

            var psi = new ProcessStartInfo(file, cmd)
            {
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                UseShellExecute = false
            };

            var p = Process.Start(psi);
            p.WaitForExit();

            var nowRunning = new HashSet<string>();

            var csv = await p.StandardOutput.ReadToEndAsync().ConfigureAwait(false);
            foreach (var line in csv.Split(Environment.NewLine))
            {
                var appId = line.LastIndexOf(',') + 1;
                if (appId == 0)
                {
                    continue;
                }

                nowRunning.Add(line.Substring(appId).Trim('"'));
            }

            if (this.previouslyRunningAppIds != null && nowRunning.SequenceEqual(this.previouslyRunningAppIds))
            {
                return;
            }

            this.previouslyRunningAppIds = nowRunning;
            this.Publish(new ActivePackageFullNames(nowRunning));


            //var path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "WindowsApps") + "\\";

            //var query = "SELECT ExecutablePath FROM Win32_Process";

            //var nowRunning = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            //using (var mos = new ManagementObjectSearcher(query))
            //{
            //    using (var moc = mos.Get())
            //    {
            //        foreach (ManagementObject mo in moc)
            //        {
            //            var executablePath = mo["ExecutablePath"];
            //            if (executablePath == null)
            //            {
            //                continue;
            //            }

            //            nowRunning.Add(executablePath?.ToString());
            //        }
            //    }
            //}

            //// var processes = Process.GetProcesses().Select(GetPathToApp).Where(p => p != null).Where(p => p.StartsWith(path, StringComparison.OrdinalIgnoreCase));
            //// var nowRunning = processes.Select(p => p.Substring(0, p.IndexOf('\\', path.Length + 1))).ToList();
            //if (this.previouslyRunningFiles != null && nowRunning.SequenceEqual(this.previouslyRunningFiles))
            //{
            //    return Task.FromResult(false);
            //}

            //this.previouslyRunningFiles = nowRunning;
            //this.Publish(new ActivePackageFullNames(this.GetActiveNames(nowRunning)));
            //return Task.FromResult(true);
        }

        private IEnumerable<string> GetActiveNames(IEnumerable<string> nowRunning)
        {
            foreach (var now in nowRunning)
            {
                var matching = this.consideredPackages.FirstOrDefault(kv => now.StartsWith(kv.Key, StringComparison.OrdinalIgnoreCase));
                if (matching.Value == null)
                {
                    continue;
                }

                yield return matching.Value;
            }
        }

        [DllImport("Kernel32.dll")]
        static extern uint QueryFullProcessImageName(IntPtr hProcess, uint flags, StringBuilder text, out uint size);

        private static string GetPathToApp(Process proc)
        {
            if (null != proc)
            {
                uint charLength = 256;
                var buffer = new StringBuilder((int)charLength);

                var success = QueryFullProcessImageName(proc.Handle, 0, buffer, out _);

                if (0 != success)
                {
                    return buffer.ToString();
                }
            }

            return null;
        }

        public Task StopListening(CancellationToken cancellationToken)
        {
            this.consideredPackages = null;
            this.timer.Elapsed -= this.TimerOnElapsed;
            this.timer.Stop();
            return Task.FromResult(true);
        }

        private void Publish(ActivePackageFullNames eventPayload)
        {
            foreach (var item in this.observers)
            {
                item.Notify(eventPayload);
            }
        }

        void IDisposable.Dispose()
        {
            foreach (var disposable in this.observers)
            {
                disposable.Dispose();
            }
        }

        private class Subscriber : IDisposable
        {
            private readonly RunningDetector parent;
            private readonly IObserver<ActivePackageFullNames> observer;

            public Subscriber(RunningDetector parent, IObserver<ActivePackageFullNames> observer)
            {
                this.parent = parent;
                this.observer = observer;
            }

            public void Notify(ActivePackageFullNames payload)
            {
                this.observer.OnNext(payload);
            }

            public void Dispose()
            {
                parent.observers.Remove(this);
            }
        }
    }
}