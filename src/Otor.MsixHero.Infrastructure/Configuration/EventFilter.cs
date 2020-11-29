using System;

namespace Otor.MsixHero.Infrastructure.Configuration
{
    [Flags]
    public enum EventFilter
    {
        Warning   = 1 << 0,
        Info      = 1 << 1,
        Error     = 1 << 2,
        Verbose   = 1 << 3,
        AllLevels = Warning | Info | Error | Verbose,
        All       = AllLevels,
        Default   = Warning | Error | Info
    }
}