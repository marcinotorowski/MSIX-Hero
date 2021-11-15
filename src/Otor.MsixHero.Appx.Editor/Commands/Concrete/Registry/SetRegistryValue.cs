using Otor.MsixHero.Registry.Parser;

namespace Otor.MsixHero.Appx.Editor.Commands.Concrete.Registry
{
    public class SetRegistryValue : IRegistryKeyCommand
    {
        public string RegistryKey { get; set; }
        
        public string RegistryValueName { get; set; }
        
        public ValueType ValueType { get; set; }
        
        public object Value { get; set; }
    }
}