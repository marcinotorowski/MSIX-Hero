using System;
using System.Diagnostics;

namespace otor.msixhero.lib.Ipc
{
    public interface  IProcessManager : IDisposable
    {
        Process Start(ProcessStartInfo info);
    }
}