using System;
using System.IO;

namespace Otor.MsixHero.Appx.Packaging.Manifest.FileReaders
{
    public interface IAppxFileReader : IDisposable
    {
        Stream GetFile(string filePath);

        bool FileExists(string filePath);

        Stream GetResource(string resourceFilePath);
    }
}