using System;
using System.IO;
using Org.BouncyCastle.Asn1;

namespace otor.msixhero.lib.BusinessLayer.Appx.Manifest.FileReaders
{
    public interface IAppxFileReader : IDisposable
    {
        Stream GetFile(string filePath);

        bool FileExists(string filePath);

        Stream GetResource(string resourceFilePath);
    }
}