using System;
using System.Threading.Tasks;

namespace Otor.MsixHero.Appx.Reader.File
{
    public interface IAppxFileViewer : IDisposable
    {
        Task<string> GetDiskPath(IAppxFileReader fileReader, string filePath);
    }
}
