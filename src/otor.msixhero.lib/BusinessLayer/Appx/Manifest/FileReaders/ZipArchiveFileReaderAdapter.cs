using System;
using System.IO;
using System.IO.Compression;
using System.Text.RegularExpressions;

namespace otor.msixhero.lib.BusinessLayer.Appx.Manifest.FileReaders
{
    public class ZipArchiveFileReaderAdapter : IAppxFileReader
    {
        private readonly ZipArchive msixPackage;
        private readonly IDisposable[] disposableStreams;

        public ZipArchiveFileReaderAdapter(ZipArchive msixPackage)
        {
            this.msixPackage = msixPackage;
        }

        public ZipArchiveFileReaderAdapter(string msixPackagePath)
        {
            if (!File.Exists(msixPackagePath))
            {
                throw new ArgumentException($"File {msixPackagePath} does not exist.");
            }

            var fileStream = File.OpenRead(msixPackagePath);
            this.msixPackage = new ZipArchive(fileStream);
            this.disposableStreams = new IDisposable[] { this.msixPackage, fileStream };
        }

        public Stream GetFile(string filePath)
        {
            var entry = msixPackage.GetEntry(filePath);
            if (entry == null)
            {
                throw new FileNotFoundException($"File {filePath} not found in MSIX package.");
            }

            return entry.Open();
        }

        public Stream GetResource(string resourceFilePath)
        {
            // @".[^\.\-]+-[^\.\-]+"

            if (this.FileExists(resourceFilePath))
            {
                return this.GetFile(resourceFilePath);
            }

            var resourceDir = Path.GetDirectoryName(resourceFilePath);

            foreach (var item in this.msixPackage.Entries)
            {
                var currentName = item.FullName;
                if (!string.IsNullOrEmpty(resourceDir))
                {
                    if (!currentName.StartsWith(resourceDir, StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }

                    currentName = currentName.Remove(0, resourceDir.Length).TrimStart('\\');
                }

                // 1) Remove quantified folder names
                currentName = Regex.Replace(currentName, @"[^\.\-]+-[^\.\-]+\\", string.Empty);
                
                // 2) Remove quantified file names
                currentName = Regex.Replace(currentName, @"\.[^\.\-]+-[^\.\-]+", string.Empty);

                if (string.Equals(currentName, resourceFilePath, StringComparison.OrdinalIgnoreCase))
                {
                    return item.Open();
                }
            }

            return null;
        }

        public bool FileExists(string filePath)
        {
            return this.msixPackage.GetEntry(filePath) != null;
        }

        public void Dispose()
        {
            foreach (var item in this.disposableStreams)
            {
                item.Dispose();
            }
        }
    }
}