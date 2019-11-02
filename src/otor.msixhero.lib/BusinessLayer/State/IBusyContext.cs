using System;
using otor.msixhero.lib.Domain;

namespace otor.msixhero.lib.BusinessLayer.State
{
    public interface IBusyContext : IProgress<ProgressData>
    {
        string Message { get; set; }

        int Progress { get; set; }
    }
}