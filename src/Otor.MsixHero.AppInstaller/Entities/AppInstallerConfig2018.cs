using System;
using System.IO;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Otor.MsixHero.AppInstaller.Entities
{
    [Serializable]
    [XmlRoot("AppInstaller", Namespace = "http://schemas.microsoft.com/appx/appinstaller/2018")]
    public class AppInstallerConfig2018 : AppInstallerConfig
    {
        public AppInstallerConfig2018()
        {
        }
        
        public AppInstallerConfig2018(AppInstallerConfig config) : base(config)
        {
        }

        public static Task<AppInstallerConfig2018> FromStream(Stream stream)
        {
            return Task.Run(() =>
            {
                var xmlSerializer = new XmlSerializer(typeof(AppInstallerConfig2018));
                var aic = (AppInstallerConfig2018)xmlSerializer.Deserialize(stream);
                return aic;
            });
        }
    }
}