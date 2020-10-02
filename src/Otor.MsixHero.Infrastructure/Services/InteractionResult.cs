using System;

namespace Otor.MsixHero.Infrastructure.Services
{
    [Flags]
    public enum InteractionResult
    {
        Cancel	= 2,
        No = 7,
        None = 0,
        OK = 1,
        Yes = 6	,
        Retry = 4,
        Close = 8
    }
}