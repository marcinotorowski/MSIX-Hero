using Microsoft.Win32;

namespace Otor.MsixHero.Registry.Reader
{
    public interface IRegValue
    {
        IRegKey Parent { get; }

        string Name { get; }

        object Value { get; }
        
        RegistryValueKind RegistryValueType { get; }

    }
}