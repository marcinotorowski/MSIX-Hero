namespace Otor.MsixHero.Appx.Packaging.Manifest.FileReaders
{
    public interface IAppxDiskFileReader : IAppxFileReader
    {
        string RootDirectory { get; }
    }
}