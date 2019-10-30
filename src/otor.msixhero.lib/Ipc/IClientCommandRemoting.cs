using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using otor.msixhero.lib.BusinessLayer.Commands;
using otor.msixhero.lib.BusinessLayer.Commands.Developer;
using otor.msixhero.lib.BusinessLayer.Commands.Grid;
using otor.msixhero.lib.BusinessLayer.Commands.Manager;
using otor.msixhero.lib.BusinessLayer.Models;

namespace otor.msixhero.lib.Ipc
{
    public interface IClientCommandRemoting
    {
        Server GetServerInstance(IAppxPackageManager packageManager);

        Client GetClientInstance();

        Task<byte[]> Handle(GetPackages command, IAppxPackageManager packageManager, CancellationToken cancellationToken = default);

        Task<byte[]> Handle(MountRegistry command, IAppxPackageManager pkgManager, CancellationToken cancellationToken = default);

        Task<byte[]> Handle(UnmountRegistry command, IAppxPackageManager pkgManager, CancellationToken cancellationToken = default);

        Task<byte[]> Handle(GetSelectionDetails command, IAppxPackageManager pkgManager, CancellationToken cancellationToken = default);

        Task<byte[]> Handle(GetUsersOfPackage command, IAppxPackageManager pkgManager, CancellationToken cancellationToken = default);

        Task<byte[]> Handle(RemovePackage command, IAppxPackageManager pkgManager, CancellationToken cancellationToken = default);
    }
}