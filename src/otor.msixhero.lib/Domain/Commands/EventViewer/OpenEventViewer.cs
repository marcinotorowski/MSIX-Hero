namespace Otor.MsixHero.Lib.Domain.Commands.EventViewer
{
    public enum EventLogType
    {
        AppXDeploymentOperational,
        AppXDeploymentServerOperational,
        AppXDeploymentServerRestricted
    }

    public class OpenEventViewer : ProxyObject
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
