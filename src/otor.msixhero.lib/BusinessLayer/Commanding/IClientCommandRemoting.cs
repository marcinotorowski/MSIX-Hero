using otor.msixhero.lib.BusinessLayer.Appx;
using otor.msixhero.lib.Infrastructure.Ipc;

namespace otor.msixhero.lib.BusinessLayer.Commanding
{
    public interface IClientCommandRemoting
    {
        Server GetServerInstance(IAppxPackageManager packageManager);

        Client GetClientInstance();

        //Task<byte[]> Handle(GetPackages command, IAppxPackageManager packageManager, CancellationToken cancellationToken = default, IProgress<ProgressData> progress = default);

        //Task Handle(MountRegistry command, IAppxPackageManager pkgManager, CancellationToken cancellationToken = default, IProgress<ProgressData> progress = default);

        //Task Handle(UnmountRegistry command, IAppxPackageManager pkgManager, CancellationToken cancellationToken = default, IProgress<ProgressData> progress = default);

        //Task<byte[]> Handle(FindUsers command, IAppxPackageManager pkgManager, CancellationToken cancellationToken = default, IProgress<ProgressData> progress = default);

        //Task<byte[]> Handle(GetUsersOfPackage command, IAppxPackageManager pkgManager, CancellationToken cancellationToken = default, IProgress<ProgressData> progress = default);

        //Task Handle(RemovePackages command, IAppxPackageManager pkgManager, CancellationToken cancellationToken = default, IProgress<ProgressData> progress = default);
    }
}