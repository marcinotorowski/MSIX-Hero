using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace otor.msixhero.lib
{
    public class AppxPackageManager : IAppxPackageManager
    {
        protected readonly IAppxPackageManager Proxy = new AppxProxyPackageManager();

        protected readonly IAppxPackageManager Local = new AppxLocalPackageManager();

        public Task MountRegistry(Package package, bool startRegedit = false)
        {
            if (UserHelper.IsAdministrator())
            {
                return this.Local.MountRegistry(package, startRegedit);
            }

            return this.Proxy.MountRegistry(package, startRegedit);
        }

        public Task UnmountRegistry(Package package)
        {
            if (UserHelper.IsAdministrator())
            {
                return this.Local.UnmountRegistry(package);
            }

            return this.Proxy.UnmountRegistry(package);
        }

        public Task RunTool(Package package, string toolName)
        {
            //if (UserHelper.IsAdministrator())
            //{
                return this.Local.RunTool(package, toolName);
            //}

            //return this.Proxy.RunTool(package, toolName);
        }

        public Task RunApp(Package package)
        {
            //if (UserHelper.IsAdministrator())
            //{
                return this.Local.RunApp(package);
            //}

            //return this.Proxy.RunApp(package);
        }

        public Task<RegistryMountState> GetRegistryMountState(string installLocation, string packageName)
        {
            return this.Local.GetRegistryMountState(installLocation, packageName);
        }

        public Task<RegistryMountState> GetRegistryMountState(Package package)
        {
            return this.Local.GetRegistryMountState(package);
        }

        public Task<IList<Package>> GetPackages(PackageFindMode mode = PackageFindMode.Auto)
        {
            switch (mode)
            {
                case PackageFindMode.CurrentUser:
                    return this.Local.GetPackages(mode);
                case PackageFindMode.Auto:
                case PackageFindMode.AllUsers:
                    if (UserHelper.IsAdministrator())
                    {
                        mode = PackageFindMode.CurrentUser;
                        return this.Local.GetPackages(mode);
                    }

                    return this.Proxy.GetPackages(mode);
            }

            throw new NotSupportedException();
        }
    }
}
