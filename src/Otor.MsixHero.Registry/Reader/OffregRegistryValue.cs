using Microsoft.Win32;

namespace Otor.MsixHero.Registry.Reader
{
    public class OffregRegistryValue : IRegValue
    {
        private readonly OffregRegistryKey parent;

        public OffregRegistryValue(OffregRegistryKey parent, string name, object value, RegistryValueKind valueKind)
        {
            this.parent = parent;
            this.Name = name;
            this.Value = value;
            this.RegistryValueType = valueKind;
        }

        public IRegKey Parent => this.parent;

        public string Name { get; }

        public object Value { get; }

        public RegistryValueKind RegistryValueType { get; }
    }
}