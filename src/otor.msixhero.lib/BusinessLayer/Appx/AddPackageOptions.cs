using System;

namespace otor.msixhero.lib.BusinessLayer.Appx
{
    [Flags]
    public enum AddPackageOptions
    {
        AllUsers = 1,
        AllowDowngrade = 2,
        KillRunningApps = 4
    }
}