using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Otor.MsixHero.Infrastructure.Progress;
using Otor.MsixHero.Infrastructure.Services;
using Otor.MsixHero.Lib.BusinessLayer.State;
using Otor.MsixHero.Lib.Domain.Commands;
using Otor.MsixHero.Lib.Domain.Commands.EventViewer;

namespace Otor.MsixHero.Lib.BusinessLayer.Executors.Client
{
    public class OpenEventViewerClientExecutor : CommandExecutor
    {
        private readonly OpenEventViewer command;

        public OpenEventViewerClientExecutor(OpenEventViewer command) : base(command)
        {
            this.command = command;
        }

        public OpenEventViewerClientExecutor(ProxyObject command) : base(command)
        {
        }

        public override Task Execute(IInteractionService interactionService, CancellationToken cancellationToken = default, IProgress<ProgressData> progressData = default)
        {
            string logName;
            switch (this.command.Type)
            {
                case EventLogType.AppXDeploymentOperational:
                    logName = @"%SystemRoot%\System32\Winevt\Logs\Microsoft-Windows-AppXDeployment%4Operational.evtx";
                    break;
                case EventLogType.AppXDeploymentServerOperational:
                    logName = @"%SystemRoot%\System32\Winevt\Logs\Microsoft-Windows-AppXDeploymentServer%4Operational.evtx";
                    break;
                case EventLogType.AppXDeploymentServerRestricted:
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
