using System;
using System.IO;

namespace Otor.MsixHero.Appx.Packaging.Manifest.FileReaders
{
    public class FileReaderFactory
    {
        public static IAppxFileReader CreateFileReader(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                throw new ArgumentNullException(nameof(path));
            }

            if (!File.Exists(path))
            {
                if (!Directory.Exists(path))
                {
                    throw new FileNotFoundException("File not found.", path);
                }

                return new DirectoryInfoFileReaderAdapter(path);
            }

            var fileName = Path.GetFileName(path);
            if (string.Equals("appxmanifest.xml", fileName, StringComparison.OrdinalIgnoreCase))
            {
                return new FileInfoFileReaderAdapter(path);
            }

            var ext = Path.GetExtension(path);
            switch (ext.ToLowerInvariant())
            {
                case ".msix":
                case ".appx":
                    return new ZipArchiveFileReaderAdapter(path);
                default:
                    return new FileInfoFileReaderAdapter(path);
            }
        }
    }
}
