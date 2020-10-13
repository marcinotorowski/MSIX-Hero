namespace Otor.MsixHero.Appx.Packaging.Manifest.Entities
{
    public abstract class AppxSource
    {
        protected AppxSource(string rootDirectory)
        {
            this.RootDirectory = rootDirectory;
        }

        public string RootDirectory { get; }
    }
}
