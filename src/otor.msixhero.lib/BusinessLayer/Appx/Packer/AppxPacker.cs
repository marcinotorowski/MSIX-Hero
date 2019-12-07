using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using otor.msixhero.lib.Infrastructure.Interop;
using otor.msixhero.lib.Infrastructure.Progress;

namespace otor.msixhero.lib.BusinessLayer.Appx.Packer
{
    public class AppxPacker : IAppxPacker
    {
        public Task Pack(string directory, string packagePath, CancellationToken cancellationToken = default, IProgress<ProgressData> progress = null)
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

            return new MsixSdkWrapper().PackPackage(directory, packagePath, cancellationToken, progress);
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