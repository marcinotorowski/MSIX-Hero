namespace Otor.MsixHero.Registry.Parser
{
    public struct RegistryEntry
    {
        public RegistryRoot Root;
        public string Key;
        public string Name;
        public ValueType Type;
        public object Value;
    }
}