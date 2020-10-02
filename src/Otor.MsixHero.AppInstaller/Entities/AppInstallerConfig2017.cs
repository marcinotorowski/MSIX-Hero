using System;
using System.IO;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Otor.MsixHero.AppInstaller.Entities
{
    [Serializable]
    [XmlRoot("AppInstaller", Namespace = "http://schemas.microsoft.com/appx/appinstaller/2017")]
    public class AppInstallerConfig2017 : AppInstallerConfig
    {
        public AppInstallerConfig2017()
        {
        }

        public AppInstallerConfig2017(AppInstallerConfig config) : base(config)
        {
            if (this.UpdateSettings?.OnLaunch != null)
            {
                // Available from 2018/0
                this.UpdateSettings.OnLaunch.UpdateBlocksActivation = false;
                this.UpdateSettings.OnLaunch.ShowPrompt = false;
            }

            if (this.UpdateSettings?.AutomaticBackgroundTask != null)
            {
                // Available from 2017/2
                this.UpdateSettings.AutomaticBackgroundTask = null;
            }

            if (this.UpdateSettings?.ForceUpdateFromAnyVersion != null)
            {
                // Available from 2017/2
                this.UpdateSettings.ForceUpdateFromAnyVersion = false;
            }
        }

        public static Task<AppInstallerConfig2017> FromStream(Stream stream)
        {
            return Task.Run(() =>
            {
                var xmlSerializer = new XmlSerializer(typeof(AppInstallerConfig2017));
                var aic = (AppInstallerConfig2017)xmlSerializer.Deserialize(stream);
                return aic;
            });
        }
    }
}