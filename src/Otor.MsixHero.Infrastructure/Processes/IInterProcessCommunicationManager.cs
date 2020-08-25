using System;
using System.IO.Pipes;
using System.Threading;
using System.Threading.Tasks;

namespace Otor.MsixHero.Infrastructure.Processes
{
    public interface IInterProcessCommunicationManager : IDisposable
    {
        Task<NamedPipeClientStream> GetCommunicationChannel(CancellationToken cancellationToken = default);

        Task<bool> Test(CancellationToken cancellationToken = default);
    }
}