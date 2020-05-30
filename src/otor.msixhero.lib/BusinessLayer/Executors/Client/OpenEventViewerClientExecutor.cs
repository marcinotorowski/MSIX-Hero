using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using otor.msixhero.lib.BusinessLayer.State;
using otor.msixhero.lib.Domain.Commands;
using otor.msixhero.lib.Domain.Commands.EventViewer;
using otor.msixhero.lib.Infrastructure;
using otor.msixhero.lib.Infrastructure.Progress;

namespace otor.msixhero.lib.BusinessLayer.Executors.Client
{
    public class OpenEventViewerClientExecutor : CommandExecutor
    {
        private readonly OpenEventViewer command;

        public OpenEventViewerClientExecutor(OpenEventViewer command, IWritableApplicationStateManager state) : base(command, state)
        {
            this.command = command;
        }

        public OpenEventViewerClientExecutor(VoidCommand command, IWritableApplicationStateManager state) : base(command, state)
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
