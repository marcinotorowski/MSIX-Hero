namespace Otor.MsixHero.Appx.Editor.Commands.Concrete.Registry
{
    public class DeleteRegistryValue : IAppxEditCommand, IRegistryKeyCommand
    {
        public string RegistryKey { get; set; }
        
        public string RegistryValueName { get; set; }
    }
}