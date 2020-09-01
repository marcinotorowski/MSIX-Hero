namespace Otor.MsixHero.Lib.Proxy.Signing.Dto
{
    public class InstallCertificateDto : ProxyObject
    {
        public InstallCertificateDto(string filePath)
        {
            this.FilePath = filePath;
        }

        public InstallCertificateDto()
        {
        }

        public string FilePath { get; set; }
    }
}
