using System;
using System.Collections.Generic;
using Otor.MsixHero.Appx.Packaging.Installation.Entities;

namespace Otor.MsixHero.Appx.Diagnostic.RunningDetector
{
    public interface IRunningAppsDetector : IObservable<ActivePackageFullNames>
    {
        void StartListening();

        IEnumerable<InstalledPackage> GetCurrentlyRunning(IEnumerable<InstalledPackage> candidates);

        IEnumerable<string> GetCurrentlyRunningPackageNames();

        void StopListening();
    }
}
