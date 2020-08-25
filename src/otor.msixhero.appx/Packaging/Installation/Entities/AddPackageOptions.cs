using System;

namespace Otor.MsixHero.Appx.Packaging.Installation.Entities
{
    [Flags]
    public enum AddPackageOptions
    {
        AllUsers = 1,
        AllowDowngrade = 2,
        KillRunningApps = 4
    }
}