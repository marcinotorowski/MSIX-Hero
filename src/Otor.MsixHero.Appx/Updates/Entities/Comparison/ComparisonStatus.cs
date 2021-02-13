using System;

namespace Otor.MsixHero.Appx.Updates.Entities.Comparison
{
    [Serializable]
    public enum ComparisonStatus
    {
        Unknown,
        New,
        Old,
        Changed,
        Unchanged,
        Duplicate
    }
}