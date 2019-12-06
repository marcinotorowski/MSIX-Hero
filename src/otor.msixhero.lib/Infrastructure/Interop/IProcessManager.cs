using System;
using System.Threading;
using System.Threading.Tasks;

namespace otor.msixhero.lib.Infrastructure.Interop
{
    public interface  IProcessManager : IDisposable
    {
        Task Connect(CancellationToken cancellationToken = default);
    }
}