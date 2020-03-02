namespace otor.msixhero.lib.Domain.Appx.Manifest.Full
{
    public class AppxService : AppxExtension
    {
        public string Name { get; set; }

        public string StartupType { get; set; }

        public string StartAccount { get; set; }

        public string Executable { get; set; }

        public string EntryPoint { get; set; }
    }
}