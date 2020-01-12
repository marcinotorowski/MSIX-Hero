using System;
using System.IO;

namespace otor.msixhero.lib.BusinessLayer.Appx.Manifest.FileReaders
{
    public class DirectoryInfoFileReaderAdapter : IAppxFileReader
    {
        private readonly IAppxFileReader adapter;

        public DirectoryInfoFileReaderAdapter(DirectoryInfo appxManifestFolder)
        {
            if (!appxManifestFolder.Exists)
            {
                throw new ArgumentException($"Directory {appxManifestFolder.FullName} does not exist.");
            }

            var appxManifest = Path.Combine(appxManifestFolder.FullName, "AppxManifest.xml");
            if (!File.Exists(appxManifest))
            {
                appxManifest = Path.Combine(appxManifestFolder.FullName, "AppxMetadata", "AppxBundleManifest.xml");
                if (!File.Exists(appxManifest))
                {
                    throw new ArgumentException("This folder does not contain APPX/MSIX package nor APPX bundle.", nameof(appxManifestFolder));
                }

                adapter = new FileInfoFileReaderAdapter(appxManifest);
            }
            else
            {
                adapter = new FileInfoFileReaderAdapter(Path.Combine(appxManifestFolder.FullName, "AppxManifest.xml"));
            }
        }

        public DirectoryInfoFileReaderAdapter(string appxManifestFolder) : this(new DirectoryInfo(appxManifestFolder))
        {
        }

        public Stream GetFile(string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
            {
                return null;
            }

            return this.adapter.GetFile(filePath);
        }

        public bool FileExists(string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
            {
                return false;
            }

            return this.adapter.FileExists(filePath);
        }

        public Stream GetResource(string resourceFilePath)
        {
            if (string.IsNullOrEmpty(resourceFilePath))
            {
                return null;
            }

            return this.adapter.GetResource(resourceFilePath);
        }

        void IDisposable.Dispose()
        {
            this.adapter.Dispose();
        }
    }
}