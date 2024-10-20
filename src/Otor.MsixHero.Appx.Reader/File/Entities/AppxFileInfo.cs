using System.IO;

namespace Otor.MsixHero.Appx.Reader.File.Entities
{
    public readonly struct AppxFileInfo
    {
        public AppxFileInfo(string fullPath, long size)
        {
            FullPath = fullPath;
            Size = size;
            Name = Path.GetFileName(fullPath);
        }

        public AppxFileInfo(FileInfo fileInfo)
        {
            FullPath = fileInfo.FullName;
            Size = fileInfo.Length;
            Name = fileInfo.Name;
        }

        public string Name { get; }

        public string FullPath { get; }

        public long Size { get; }
    }
}
