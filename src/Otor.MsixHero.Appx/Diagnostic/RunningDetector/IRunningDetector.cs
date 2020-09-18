using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Otor.MsixHero.Appx.Packaging.Installation.Entities;

namespace Otor.MsixHero.Appx.Diagnostic.RunningDetector
{
    public interface IRunningDetector : IObservable<ActivePackageFullNames>
    {
        Task Listen(IList<InstalledPackage> installedPackages, CancellationToken cancellationToken);

        Task StopListening(CancellationToken cancellationToken);
    }
}
