using System;

namespace Otor.MsixHero.Appx.Packaging.Manifest.Entities.Summary
{
    [Flags]
    public enum AppxManifestSummaryBuilderMode
    {
        Identity = 1,
        Applications = 2,
        Properties = 4,
        Minimal = Identity | Applications | Properties
    }
}