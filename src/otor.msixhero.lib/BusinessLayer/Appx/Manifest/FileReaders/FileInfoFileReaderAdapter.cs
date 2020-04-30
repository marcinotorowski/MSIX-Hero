using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace otor.msixhero.lib.BusinessLayer.Appx.Manifest.FileReaders
{
    public class FileInfoFileReaderAdapter : IAppxDiskFileReader
    {
        private readonly FileInfo appxManifestFile;

        public FileInfoFileReaderAdapter(FileInfo appxManifestFile)
        {
            if (!appxManifestFile.Exists)
            {
                throw new ArgumentException($"File {appxManifestFile.FullName} does not exist.", nameof(appxManifestFile));
            }

            this.RootDirectory = appxManifestFile.DirectoryName;
            this.appxManifestFile = appxManifestFile;
        }

        public FileInfoFileReaderAdapter(string appxManifestFile)
        {
            if (string.IsNullOrEmpty(appxManifestFile))
            {
                this.RootDirectory = null;
                this.appxManifestFile = null;
            }
            else
            {
                var fileInfo = new FileInfo(appxManifestFile);
                this.RootDirectory = fileInfo.DirectoryName;
                this.appxManifestFile = fileInfo;
            }
        }

        public string RootDirectory { get; }

        public Stream GetFile(string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
            {
                return null;
            }

            // ReSharper disable once PossibleNullReferenceException
            return File.OpenRead(Path.Combine(this.appxManifestFile.Directory.FullName, filePath));
        }

        public Stream GetResource(string resourceFilePath)
        {
            if (string.IsNullOrEmpty(resourceFilePath))
            {
                return null;
            }

            if (this.FileExists(resourceFilePath))
            {
                return this.GetFile(resourceFilePath);
            }

            var fileName = Path.GetFileName(resourceFilePath);
            var extension = Path.GetExtension(resourceFilePath);
            var resourceDir = Path.GetDirectoryName(resourceFilePath);

            var dirsToTry = new Queue<string>();
            dirsToTry.Enqueue(string.IsNullOrEmpty(resourceDir) ? this.appxManifestFile.DirectoryName : Path.Combine(this.appxManifestFile.DirectoryName, resourceDir));

            while (dirsToTry.Any())
            {
                var dequeued = dirsToTry.Dequeue();
                var dirInfo = new DirectoryInfo(dequeued);
                if (!dirInfo.Exists)
                {
                    continue;
                }

                var matchingFiles = dirInfo.EnumerateFiles(Path.GetFileNameWithoutExtension(fileName) + "*" + extension);
                foreach(var matchingFile in matchingFiles)
                {
                    var name = Regex.Replace(matchingFile.Name, @"\.[^\.\-]+-[^\.\-]+", string.Empty);
                    if (string.Equals(name, fileName, StringComparison.OrdinalIgnoreCase))
                    {
                        return this.GetFile(Path.GetRelativePath(this.appxManifestFile.DirectoryName, matchingFile.FullName));
                    }
                }

                var matchingDirectories = dirInfo.EnumerateDirectories().Where(d => Regex.IsMatch(dirInfo.Name, @".[^\.\-]+-[^\.\-]+"));
                foreach (var matchingDirectory in matchingDirectories)
                {
                    dirsToTry.Enqueue(matchingDirectory.FullName);
                }
            }

            return null;
        }

        public bool FileExists(string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
            {
                return false;
            }

            if (this.appxManifestFile?.Directory?.FullName == null)
            {
                return false;
            }

            // ReSharper disable once PossibleNullReferenceException
            return File.Exists(Path.Combine(this.appxManifestFile.Directory.FullName, filePath));
        }

        void IDisposable.Dispose()
        {
        }
    }
}