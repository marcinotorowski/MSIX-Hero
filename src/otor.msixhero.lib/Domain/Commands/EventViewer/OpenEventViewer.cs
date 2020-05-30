using System.Diagnostics;

namespace otor.msixhero.lib.Domain.Commands.EventViewer
{
    public enum EventLogType
    {
        AppXDeploymentOperational,
        AppXDeploymentServerOperational,
        AppXDeploymentServerRestricted
    }

    public class OpenEventViewer : VoidCommand
    {
        public OpenEventViewer()
        {
        }

        public OpenEventViewer(EventLogType type)
        {
            this.Type = type;
        }

        public EventLogType Type { get; set; }
    }
}
