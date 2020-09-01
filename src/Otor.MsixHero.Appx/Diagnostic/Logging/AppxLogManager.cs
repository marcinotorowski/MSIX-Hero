using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Otor.MsixHero.Appx.Diagnostic.Logging.Entities;
using Otor.MsixHero.Appx.Diagnostic.Logging.Enums;
using Otor.MsixHero.Infrastructure.Logging;
using Otor.MsixHero.Infrastructure.Progress;
using Otor.MsixHero.Infrastructure.ThirdParty.PowerShell;

namespace Otor.MsixHero.Appx.Diagnostic.Logging
{
    public class AppxLogManager : IAppxLogManager
    {
        private static readonly ILog Logger = LogManager.GetLogger();

        public async Task<List<Log>> GetLogs(int maxCount, CancellationToken cancellationToken = default, IProgress<ProgressData> progress = default)
        {
            Logger.Info("Getting last {0} log files...", maxCount);

            var factory = new LogFactory();
            var allLogs = new List<Log>();

            using var ps = await PowerShellSession.CreateForModule().ConfigureAwait(false);
            using var script = ps.AddScript("(Get-WinEvent -ListLog *Microsoft-Windows-*Appx* -ErrorAction SilentlyContinue).ProviderNames");
            using var logs = await ps.InvokeAsync().ConfigureAwait(false);

            var logNames = logs.ToArray();

            var progresses = new IProgress<ProgressData>[logNames.Length];

            using (var wrapperProgress = new WrappedProgress(progress ?? new Progress<ProgressData>()))
            {
                for (var index = 0; index < logNames.Length; index++)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    progresses[index] = wrapperProgress.GetChildProgress(100.0);
                }

                for (var index = 0; index < logNames.Length; index++)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    var item = logNames[index];
                    var itemProgress = progresses[index];
                    var logName = (string)item.BaseObject;
                    itemProgress.Report(new ProgressData(0, "Reading " + logName + "..."));

                    cancellationToken.ThrowIfCancellationRequested();

                    using var psLocal = await PowerShellSession.CreateForModule().ConfigureAwait(false);
                    using var scriptLocal = psLocal.AddScript("Get-WinEvent -ProviderName " + logName + " -MaxEvents " + maxCount + " -ErrorAction SilentlyContinue");
                    using var logItems = await psLocal.InvokeAsync(itemProgress).ConfigureAwait(false);

                    allLogs.AddRange(logItems.Select(log => factory.CreateFromPowerShellObject(log)));
                }
            }

            return allLogs.OrderByDescending(l => l.DateTime).Take(maxCount).ToList();
        }

        public Task OpenEventViewer(EventLogCategory type, CancellationToken cancellationToken = default, IProgress<ProgressData> progress = default)
        {
            string logName;
            switch (type)
            {
                case EventLogCategory.AppXDeploymentOperational:
                    logName = @"%SystemRoot%\System32\Winevt\Logs\Microsoft-Windows-AppXDeployment%4Operational.evtx";
                    break;
                case EventLogCategory.AppXDeploymentServerOperational:
                    logName = @"%SystemRoot%\System32\Winevt\Logs\Microsoft-Windows-AppXDeploymentServer%4Operational.evtx";
                    break;
                case EventLogCategory.AppXDeploymentServerRestricted:
                    logName = @"%SystemRoot%\System32\Winevt\Logs\Microsoft-Windows-AppXDeploymentServer%4Restricted.evtx";
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            logName = Environment.ExpandEnvironmentVariables(logName);
            var process = new ProcessStartInfo("eventvwr", $"/l:\"{logName}\"")
            {
                Verb = "runas",
                UseShellExecute = true
            };

            Process.Start(process);
            return Task.FromResult(true);
        }
    }
}