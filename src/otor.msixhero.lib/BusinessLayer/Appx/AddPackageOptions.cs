using System;

namespace otor.msixhero.lib.BusinessLayer.Appx
{
    [Flags]
    public enum AddPackageOptions
    {
        AllUsers,
        AllowDowngrade,
        KillRunningApps
    }
}