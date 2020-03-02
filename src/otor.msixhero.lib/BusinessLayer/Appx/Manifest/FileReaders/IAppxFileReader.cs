using System;
using System.IO;

namespace otor.msixhero.lib.BusinessLayer.Appx.Manifest.FileReaders
{
    public interface IAppxFileReader : IDisposable
    {
        Stream GetFile(string filePath);

        bool FileExists(string filePath);

        Stream GetResource(string resourceFilePath);
    }

    public interface IAppxDiskFileReader : IAppxFileReader
    {
        string RootDirectory { get; }
    }
}