using System;

namespace otor.msixhero.lib.Infrastructure.Progress
{
    public interface IBusyContext : IProgress<ProgressData>
    {
        OperationType Type { get; }

        string Message { get; set; }

        int Progress { get; set; }
    }
}