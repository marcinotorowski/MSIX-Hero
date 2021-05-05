namespace Otor.MsixHero.Appx.Packaging.Registry
{
    public static class AppxRegistryRoots
    {
        public const string Root = @"Root\Registry\";
        
        // ReSharper disable once IdentifierTypo
        // ReSharper disable once InconsistentNaming
        public const string HKLM = Root + @"Machine\";

        // ReSharper disable once IdentifierTypo
        // ReSharper disable once InconsistentNaming
        public const string HKCU = Root + @"User\";
    }
}