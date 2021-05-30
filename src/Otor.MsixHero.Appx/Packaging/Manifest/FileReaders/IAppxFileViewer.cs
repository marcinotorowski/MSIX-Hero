using System;
using System.Threading.Tasks;

namespace Otor.MsixHero.Appx.Packaging.Manifest.FileReaders
{
    public interface IAppxFileViewer : IDisposable
    {
        Task<string> GetDiskPath(IAppxFileReader fileReader, string filePath);
    }
}
