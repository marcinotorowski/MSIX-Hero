using System.Collections.Generic;

namespace otor.msihero.lib
{
    public interface IAppxPackageManager
    {
        void MountRegistry(Package package, bool startRegedit = false);
        void UnmountRegistry(Package package);

        void RunTool(Package package, string toolName);

        void RunApp(Package package);

        RegistryMountState GetRegistryMountState(string installLocation, string packageName);

        RegistryMountState GetRegistryMountState(Package package);

        IEnumerable<Package> GetPackages(PackageFindMode mode = PackageFindMode.Auto, bool elevateIfRequired = true);
    }
}