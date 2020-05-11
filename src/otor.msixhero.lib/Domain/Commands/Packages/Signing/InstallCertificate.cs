namespace otor.msixhero.lib.Domain.Commands.Packages.Signing
{
    public class InstallCertificate : VoidCommand
    {
        public InstallCertificate(string filePath)
        {
            this.FilePath = filePath;
        }

        public InstallCertificate()
        {
        }

        public string FilePath { get; set; }
    }
}
