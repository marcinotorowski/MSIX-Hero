using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using otor.msixhero.lib.BusinessLayer.Commands;
using otor.msixhero.lib.BusinessLayer.Commands.Developer;
using otor.msixhero.lib.BusinessLayer.Commands.Grid;
using otor.msixhero.lib.BusinessLayer.Commands.Manager;
using otor.msixhero.lib.BusinessLayer.Models;
using otor.msixhero.lib.Domain;
using otor.msixhero.lib.Managers;

namespace otor.msixhero.lib.Ipc
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