using System.Collections.Generic;
using System.Linq;
using Otor.MsixHero.Appx.Packaging.Installation.Entities;

namespace Otor.MsixHero.Appx.Diagnostic.RunningDetector
{
    public class ActivePackageFullNames
    {
        public ActivePackageFullNames(IEnumerable<InstalledPackage> running)
        {
            this.Running = running.Select(r => r.PackageId).ToList();
        }

        public ActivePackageFullNames(IEnumerable<string> running)
        {
            this.Running = running.ToList();
        }

        public IReadOnlyList<string> Running { get; }
    }
}