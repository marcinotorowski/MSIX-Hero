using System.Xml.Serialization;

namespace Otor.MsixHero.AppInstaller.Entities
{
    public class UpdateSettings
    {
        public UpdateSettings()
        {
            this.OnLaunch = new OnLaunchSettings();
        }

        [XmlElement("OnLaunch")]
        public OnLaunchSettings OnLaunch { get; set; }

        [XmlElement("ForceUpdateFromAnyVersion")]
        public bool ForceUpdateFromAnyVersion { get; set; }

        [XmlElement("AutomaticBackgroundTask")]
        public AutomaticBackgroundTaskSettings AutomaticBackgroundTask { get; set; }
    }
}