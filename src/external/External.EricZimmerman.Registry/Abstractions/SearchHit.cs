namespace External.EricZimmerman.Registry.Abstractions
{
    using External.EricZimmerman.Registry.Other;

    public class SearchHit
    {
        public enum HitType
        {
            KeyName = 0,
            ValueName = 1,
            ValueData = 2,
            ValueSlack = 3,
            LastWrite = 5,
            Base64 = 6
        }
        public SearchHit(RegistryKey key, KeyValue value, string hitstring, string decodedValue, HitType hitLocation)
        {
            this.Key = key;
            this.Value = value;
            this.HitString = hitstring;
            this.DecodedValue = decodedValue;
            this.HitLocation = hitLocation;
        }

        public RegistryKey Key { get; }
        public KeyValue Value { get; }
        public string HitString { get; }
        public string DecodedValue { get; }

        public HitType HitLocation { get; set; }

        public bool StripRootKeyName { get; set; }

        public override string ToString()
        {
            var kp = this.Key.KeyPath;
            if (this.StripRootKeyName)
            {
                kp = Helpers.StripRootKeyNameFromKeyPath(kp);
            }

            if (this.Value != null)
            {
                return $"{kp} Hit string: {this.HitString} Value: {this.Value.ValueName}";
            }

            return $"{kp} Hit string: {this.HitString}";
        }
    }
}