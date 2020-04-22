namespace otor.msixhero.lib.Domain.Appx.Manifest.Full
{
    public enum CapabilityType
    {
        General,
        Restricted,
        Device,
        Custom
    }

    public class AppxCapability
    {
        public CapabilityType Type { get; set; }

        public string Name { get; set; }
    }
}
