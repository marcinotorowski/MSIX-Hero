using otor.msixhero.lib.Domain.Appx.UpdateImpact.ComparePackage;

namespace otor.msixhero.lib.Domain.Appx.UpdateImpact
{
    public class UpdateImpactResult
    {
        public SdkComparePackage Comparison { get; set; }

        public UpdateImpactPackage NewPackage { get; set; }

        public UpdateImpactPackage OldPackage { get; set; }
    }
}
