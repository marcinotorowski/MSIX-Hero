using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using otor.msixhero.lib.BusinessLayer.Commands;
using otor.msixhero.lib.BusinessLayer.Models;

namespace otor.msixhero.lib.Ipc
{
    public interface IClientCommandRemoting
    {
        Task<SelectionDetails> Execute(GetSelectionDetails command, CancellationToken cancellationToken = default);

        Task<List<Package>> Execute(GetPackages command, CancellationToken cancellationToken = default);

        Task<bool> Execute(UnmountRegistry command, CancellationToken cancellationToken = default);

        Task<bool> Execute(MountRegistry command, CancellationToken cancellationToken = default);

        Task<bool> Execute(RemovePackage command, CancellationToken cancellationToken = default);

        Task<List<User>> Execute(GetUsersOfPackage command, CancellationToken cancellationToken = default);



        Task<byte[]> Handle(GetPackages command, IAppxPackageManager packageManager, CancellationToken cancellationToken = default);

        Task<byte[]> Handle(MountRegistry command, IAppxPackageManager pkgManager, CancellationToken cancellationToken = default);

        Task<byte[]> Handle(UnmountRegistry command, IAppxPackageManager pkgManager, CancellationToken cancellationToken = default);

        Task<byte[]> Handle(GetSelectionDetails command, IAppxPackageManager pkgManager, CancellationToken cancellationToken = default);

        Task<byte[]> Handle(GetUsersOfPackage command, IAppxPackageManager pkgManager, CancellationToken cancellationToken = default);

        Task<byte[]> Handle(RemovePackage command, IAppxPackageManager pkgManager, CancellationToken cancellationToken = default);
    }
}