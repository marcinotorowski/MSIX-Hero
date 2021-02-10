namespace Otor.MsixHero.Appx.Updates.Entities.Comparison
{
    public interface IImpactedFiles
    {
        long UpdateImpact { get; set; }

        long ActualUpdateImpact { get; set; }

        long SizeDifference { get; set; }
    }
}