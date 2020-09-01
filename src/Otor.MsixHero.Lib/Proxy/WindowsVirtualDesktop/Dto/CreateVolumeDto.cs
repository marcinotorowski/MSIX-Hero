namespace Otor.MsixHero.Lib.Proxy.WindowsVirtualDesktop.Dto
{
    public class CreateVolumeDto : ProxyObject
    {
        public CreateVolumeDto(string packagePath, string vhdPath)
        {
            PackagePath = packagePath;
            VhdPath = vhdPath;
        }

        public CreateVolumeDto()
        {
        }

        public string PackagePath { get; set; }

        public string VhdPath { get; set; }

        public uint SizeInMegaBytes { get; set; }

        public bool GenerateScripts { get; set; }

        public bool ExtractCertificate { get; set; }
    }
}
