using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Serialization;
using otor.msixhero.lib.Domain.Appx.AppInstaller;
using otor.msixhero.lib.Infrastructure.Progress;

namespace otor.msixhero.lib.BusinessLayer.Appx.AppInstaller
{
    public class AppInstallerCreator : IAppInstallerCreator
    {
        public async Task Create(AppInstallerConfig config, string file, CancellationToken cancellationToken = default, IProgress<ProgressData> progress = default)
        {
            var fileInfo = new FileInfo(file);
            // ReSharper disable once PossibleNullReferenceException
            if (!fileInfo.Directory.Exists)
            {
                fileInfo.Directory.Create();
            }

            var xmlSerializer = new XmlSerializer(typeof(AppInstallerConfig));
            
            var sb = new StringBuilder();
            await using var textWriter = new StringWriter();
            xmlSerializer.Serialize(textWriter, config);
            await File.WriteAllTextAsync(file, textWriter.ToString(), cancellationToken).ConfigureAwait(false);
        }

        public int GetMinimumSupportedWindowsVersion(AppInstallerConfig config)
        {
            var maximum = 1709;

            if (config.UpdateSettings != null)
            {
                if (config.UpdateSettings.ForceUpdateFromAnyVersion)
                {
                    maximum = Math.Max(maximum, 1809);
                }

                if (config.UpdateSettings.AutomaticBackgroundTask != null)
                {
                    maximum = Math.Max(maximum, 1803);
                }

                if (config.UpdateSettings.OnLaunch != null)
                {
                    if (config.UpdateSettings.OnLaunch.UpdateBlocksActivation || config.UpdateSettings.OnLaunch.ShowPrompt)
                    {
                        maximum = Math.Max(maximum, 1903);
                    }
                }
            }

            return maximum;
        }
    }
}