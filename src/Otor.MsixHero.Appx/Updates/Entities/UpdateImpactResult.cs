using Otor.MsixHero.Appx.Updates.Serialization.ComparePackage;

namespace Otor.MsixHero.Appx.Updates.Entities
{
    public class UpdateImpactResult
    {
        public SdkComparePackage Comparison { get; set; }

        public UpdateImpactPackage NewPackage { get; set; }

        public UpdateImpactPackage OldPackage { get; set; }
    }
}
