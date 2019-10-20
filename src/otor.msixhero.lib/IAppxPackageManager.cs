using System.Collections.Generic;

namespace otor.msihero.lib
{
    public interface IAppxPackageManager
    {
        void RunTool(Package package, string toolName);
        void RunApp(Package package);
        IEnumerable<Package> GetPackages(PackageFindMode mode = PackageFindMode.Auto, bool elevateIfRequired = true);
    }
}