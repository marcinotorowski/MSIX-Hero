using System.Threading.Tasks;

namespace Otor.MsixHero.Appx.Packaging.Manifest.FileReaders
{
    public interface IAppxFileViewer
    {
        Task<string> GetPath(IAppxFileReader fileReader, string filePath);
    }
}
