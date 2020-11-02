using System;
using Otor.MsixHero.Infrastructure.Progress;

namespace Otor.MsixHero.Lib.Infrastructure.Progress
{
    public interface IBusyContext : IProgress<ProgressData>
    {
        OperationType Type { get; }

        string Message { get; set; }

        int Progress { get; set; }
    }
}