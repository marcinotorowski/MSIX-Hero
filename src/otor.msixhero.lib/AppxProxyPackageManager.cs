using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using otor.msixhero.lib.Ipc;

namespace otor.msixhero.lib
{
    //public class AppxProxyPackageManager : IAppxPackageManager
    //{
    //    public async Task MountRegistry(Package package, bool startRegedit = false)
    //    {
    //        var client = new Client();
    //        await client.Execute(new MountRegistryCommand { HiveName = package.Name, Source = System.IO.Path.Combine(package.InstallLocation, "Registry.dat") });
    //    }

    //    public async Task UnmountRegistry(Package package)
    //    {

    //        var client = new Client();
    //        await client.Execute(new UnmountRegistryCommand { HiveName = package.Name });
    //    }

    //    public Task RunTool(Package package, string toolName)
    //    {
    //        throw new NotImplementedException();
    //    }

    //    public Task RunApp(Package package)
    //    {
    //        throw new NotImplementedException();
    //    }

    //    public Task<RegistryMountState> GetRegistryMountState(string installLocation, string packageName)
    //    {
    //        throw new NotSupportedException();
    //    }

    //    public Task<RegistryMountState> GetRegistryMountState(Package package)
    //    {
    //        throw new NotSupportedException();
    //    }

    //    public async Task<IList<Package>> GetPackages(PackageFindMode mode = PackageFindMode.Auto)
    //    {
    //        var client = new Client();
    //        var response = await client.Execute(new GetPackagesCommand() { Mode = mode });
    //        return response.Result;
    //    }
    // }
}