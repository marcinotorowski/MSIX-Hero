using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Otor.MsixHero.Infrastructure.Processes.Ipc;
using Otor.MsixHero.Infrastructure.Progress;

namespace Otor.MsixHero.Infrastructure.Processes.SelfElevation.Enums
{
    public abstract class SelfElevationProxyReceiver<T> : ISelfElevationProxyReceiver where T : ISelfElevationAware
    {
        protected readonly T SelfElevationAwareObject;

        protected SelfElevationProxyReceiver(T selfElevationAware)
        {
            this.SelfElevationAwareObject = selfElevationAware;
        }

        public abstract IEnumerable<Type> GetSupportedProxiedObjectTypes();

        public abstract Task<TCommandResult> Get<TCommandResult>(IProxyObjectWithOutput<TCommandResult> command, CancellationToken cancellationToken = default, IProgress<ProgressData> progress = null);

        public abstract Task Invoke(IProxyObject command, CancellationToken cancellationToken = default, IProgress<ProgressData> progress = null);
    }
}
