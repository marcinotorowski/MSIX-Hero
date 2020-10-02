using System;
using System.IO;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Otor.MsixHero.AppInstaller.Entities
{
    [Serializable]
    [XmlRoot("AppInstaller", Namespace = "http://schemas.microsoft.com/appx/appinstaller/2017/2")]
    public class AppInstallerConfig20172 : AppInstallerConfig
    {
        public AppInstallerConfig20172()
        {
        }

        public AppInstallerConfig20172(AppInstallerConfig config) : base(config)
        {
            if (this.UpdateSettings?.OnLaunch != null)
            {
                // Available from 2018/0
                this.UpdateSettings.OnLaunch.UpdateBlocksActivation = false;
                this.UpdateSettings.OnLaunch.ShowPrompt = false;
            }
        }

        public static Task<AppInstallerConfig20172> FromStream(Stream stream)
        {
            return Task.Run(() =>
            {
                var xmlSerializer = new XmlSerializer(typeof(AppInstallerConfig20172));
                var aic = (AppInstallerConfig20172)xmlSerializer.Deserialize(stream);
                return aic;
            });
        }
    }
}