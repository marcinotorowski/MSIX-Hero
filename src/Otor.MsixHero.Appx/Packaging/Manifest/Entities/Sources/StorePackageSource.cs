namespace Otor.MsixHero.Appx.Packaging.Manifest.Entities.Sources
{
    public class StorePackageSource : AppxSource
    {
        public StorePackageSource(string family, string rootDirectory) : base(rootDirectory)
        {
            this.FamilyName = family;
        }

        public string FamilyName { get; }
    }
}