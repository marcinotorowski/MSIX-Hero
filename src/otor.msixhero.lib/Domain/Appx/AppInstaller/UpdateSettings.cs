using System.Xml.Serialization;

namespace otor.msixhero.lib.Domain.Appx.AppInstaller
{
    public class UpdateSettings
    {
        public UpdateSettings()
        {
            this.OnLaunch = new OnLaunchSettings();
        }

        [XmlElement("OnLaunch")]
        public OnLaunchSettings OnLaunch { get; set; }

        [XmlElement]
        public bool ForceUpdateFromAnyVersion { get; set; }
    }
}