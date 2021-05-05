using System.IO;

namespace Otor.MsixHero.Appx.Packaging.Manifest.FileReaders
{
    public readonly struct AppxFileInfo
    {
        public AppxFileInfo(string fullPath, long size)
        {
            this.FullPath = fullPath;
            this.Size = size;
            this.Name = System.IO.Path.GetFileName(fullPath);
        }

        public AppxFileInfo(FileInfo fileInfo)
        {
            this.FullPath = fileInfo.FullName;
            this.Size = fileInfo.Length;
            this.Name = fileInfo.Name;
        }

        public string Name { get; }

        public string FullPath { get; }

        public long Size { get; }
    }
}
