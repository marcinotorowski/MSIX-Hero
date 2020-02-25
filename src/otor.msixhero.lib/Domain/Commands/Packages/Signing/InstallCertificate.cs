namespace otor.msixhero.lib.Domain.Commands.Packages.Signing
{
    public class InstallCertificate : SelfElevatedCommand
    {
        public InstallCertificate(string filePath)
        {
            this.FilePath = filePath;
        }

        public InstallCertificate()
        {
        }

        public string FilePath { get; set; }

        public override SelfElevationType RequiresElevation => SelfElevationType.RequireAdministrator;
    }
}
