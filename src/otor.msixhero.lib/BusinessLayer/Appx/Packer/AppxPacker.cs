using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using otor.msixhero.lib.BusinessLayer.Appx.Detection;
using otor.msixhero.lib.Infrastructure.Progress;
using otor.msixhero.lib.Infrastructure.Wrappers;

namespace otor.msixhero.lib.BusinessLayer.Appx.Packer
{
    public class AppxPacker : IAppxPacker
    {
        public async Task Pack(string directory, string packagePath, AppxPackerOptions options = 0, CancellationToken cancellationToken = default, IProgress<ProgressData> progress = null)
        {
            if (!Directory.Exists(directory))
            {
                throw new DirectoryNotFoundException($"Folder {directory} does not exist.");
            }

            var fileInfo = new FileInfo(packagePath);
            if (fileInfo.Directory == null)
            {
                throw new ArgumentException($"File path {packagePath} is not supported.", nameof(packagePath));
            }

            if (!fileInfo.Directory.Exists)
            {
                fileInfo.Directory.Create();
            }

            var tempFile = Path.GetTempFileName();
            var tempManifest = Path.GetTempFileName();
            try
            {
                var inputDirectory = new DirectoryInfo(directory);

                var stringBuilder = new StringBuilder();
                stringBuilder.AppendLine("[Files]");

                foreach (var item in inputDirectory.EnumerateFiles("*", SearchOption.AllDirectories))
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    var relativePath = Path.GetRelativePath(directory, item.FullName);

                    if (string.IsNullOrEmpty(relativePath))
                    {
                        continue;
                    }

                    // ReSharper disable once ConvertIfStatementToConditionalTernaryExpression
                    if (string.Equals("AppxManifest.xml", relativePath, StringComparison.OrdinalIgnoreCase))
                    {
                        stringBuilder.AppendLine($"\"{tempManifest}\"\t\"{relativePath}\"");
                        item.CopyTo(tempManifest, true);
                        continue;
                    }

                    if (
                        string.Equals("AppxBlockMap.xml", relativePath, StringComparison.OrdinalIgnoreCase) ||
                        string.Equals("AppxSignature.p7x", relativePath, StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }
                    
                    stringBuilder.AppendLine($"\"{item.FullName}\"\t\"{relativePath}\"");
                }

                await File.WriteAllTextAsync(tempFile, stringBuilder.ToString(), Encoding.UTF8, cancellationToken).ConfigureAwait(false);

                var xmlDocument = XDocument.Load(tempManifest);
                var injector = new MsixHeroBrandingInjector();
                injector.Inject(xmlDocument);

                cancellationToken.ThrowIfCancellationRequested();

                await File.WriteAllTextAsync(tempManifest, xmlDocument.ToString(), Encoding.UTF8, cancellationToken).ConfigureAwait(false);

                cancellationToken.ThrowIfCancellationRequested();

                var compress = !options.HasFlag(AppxPackerOptions.NoCompress);
                var validate = !options.HasFlag(AppxPackerOptions.NoValidation);

                await new MsixSdkWrapper().PackPackageFiles(tempFile, packagePath, compress, validate, cancellationToken, progress).ConfigureAwait(false);
            }
            finally
            {
                if (File.Exists(tempFile))
                {
                    File.Delete(tempFile);
                }

                if (File.Exists(tempManifest))
                {
                    File.Delete(tempManifest);
                }
            }
        }

        public Task Unpack(string packagePath, string directory, CancellationToken cancellationToken = default, IProgress<ProgressData> progress = null)
        {
            if (!File.Exists(packagePath))
            {
                throw new FileNotFoundException($"File {packagePath} does not exist.");
            }

            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            return new MsixSdkWrapper().UnpackPackage(packagePath, directory, cancellationToken, progress);
        }
    }
}