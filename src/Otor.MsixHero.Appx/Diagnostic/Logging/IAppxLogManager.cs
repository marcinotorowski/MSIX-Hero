using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Otor.MsixHero.Appx.Diagnostic.Logging.Entities;
using Otor.MsixHero.Appx.Diagnostic.Logging.Enums;
using Otor.MsixHero.Infrastructure.Processes.SelfElevation;
using Otor.MsixHero.Infrastructure.Progress;

namespace Otor.MsixHero.Appx.Diagnostic.Logging
{
    public interface IAppxLogManager : ISelfElevationAware
    {
        Task<List<Log>> GetLogs(int maxCount, CancellationToken cancellationToken = default, IProgress<ProgressData> progress = default);

        Task OpenEventViewer(EventLogCategory type, CancellationToken cancellationToken = default, IProgress<ProgressData> progress = default);
    }
}