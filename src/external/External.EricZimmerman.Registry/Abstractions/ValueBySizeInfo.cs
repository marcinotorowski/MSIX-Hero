namespace External.EricZimmerman.Registry.Abstractions
{
    public class ValueBySizeInfo
    {
        public ValueBySizeInfo(RegistryKey key, KeyValue value)
        {
            this.Key = key;
            this.Value = value;
        }

        public RegistryKey Key { get; }
        public KeyValue Value { get; }
    }
}