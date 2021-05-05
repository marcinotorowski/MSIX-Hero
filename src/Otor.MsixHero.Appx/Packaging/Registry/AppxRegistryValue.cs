namespace Otor.MsixHero.Appx.Packaging.Registry
{
    public struct AppxRegistryValue
    {
        public string Name;

        public string Type;

        public string Data;

        public string Path;

        public override string ToString()
        {
            return $"{this.Name} = [{this.Type}] {this.Data}";
        }
    }
}