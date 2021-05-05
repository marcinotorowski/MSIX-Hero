namespace Otor.MsixHero.Appx.Packaging.Registry
{
    public struct AppxRegistryKey
    {
        public string Path;
        
        public bool HasSubKeys;

        public bool HasSubValues;

        public override string ToString()
        {
            return this.Path;
        }
    }
}