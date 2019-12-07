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
            await File.WriteAllTextAsync(file, sb.ToString(), cancellationToken).ConfigureAwait(false);
        }
    }
}