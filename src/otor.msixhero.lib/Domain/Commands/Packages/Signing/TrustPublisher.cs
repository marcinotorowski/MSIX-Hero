namespace otor.msixhero.lib.Domain.Commands.Packages.Signing
{
    public class TrustPublisher : VoidCommand
    {
        public TrustPublisher(string filePath)
        {
            this.FilePath = filePath;
        }

        public TrustPublisher()
        {
        }

        public string FilePath { get; set; }
    }
}
