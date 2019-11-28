using System;
using System.Diagnostics;

namespace otor.msixhero.lib.Infrastructure.Interop
{
    public interface  IProcessManager : IDisposable
    {
        Process Start(ProcessStartInfo info);
    }
}