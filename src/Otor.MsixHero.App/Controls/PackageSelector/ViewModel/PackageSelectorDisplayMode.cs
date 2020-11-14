using System;

namespace Otor.MsixHero.App.Controls.PackageSelector.ViewModel
{
    [Flags]
    public enum PackageSelectorDisplayMode
    {
        ShowDisplayName = 1,
        ShowActualName = 2,
        AllowPackages = 4,
        AllowBundles = 8,
        AllowManifests = 16,
        AllowAllPackageTypes = AllowPackages | AllowBundles | AllowManifests,
        ShowTypeSelector = 32,
        AllowChanging = 64,
        AllowBrowsing = 128,
        RequireVersion = 256,
        RequireArchitecture = 512,
        RequireFullIdentity = RequireVersion | RequireArchitecture
    }
}