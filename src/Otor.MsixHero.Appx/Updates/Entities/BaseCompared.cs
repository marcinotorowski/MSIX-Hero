namespace Otor.MsixHero.Appx.Updates.Entities
{
    public abstract class BaseCompared
    {
        public long BaseTotalSize { get; set; }

        public long TargetTotalSize { get; set; }

        public long UpdateImpact { get; set; }

        public long ActualUpdateImpact { get; set; }

        public long SizeDifference { get; set; }
    }
}