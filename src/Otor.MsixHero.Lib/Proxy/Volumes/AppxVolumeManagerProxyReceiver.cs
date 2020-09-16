using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Otor.MsixHero.Appx.Volumes;
using Otor.MsixHero.Infrastructure.Processes.Ipc;
using Otor.MsixHero.Infrastructure.Processes.SelfElevation.Enums;
using Otor.MsixHero.Infrastructure.Progress;
using Otor.MsixHero.Lib.Proxy.Volumes.Dto;

namespace Otor.MsixHero.Lib.Proxy.Volumes
{
    public class AppxVolumeManagerProxyReceiver : SelfElevationProxyReceiver<IAppxVolumeManager>
    {
        public AppxVolumeManagerProxyReceiver(IAppxVolumeManager selfElevationAware) : base(selfElevationAware)
        {
        }

        public override IEnumerable<Type> GetSupportedProxiedObjectTypes()
        {
            yield return typeof(AddDto);
            yield return typeof(MovePackageToVolumeDto);
            yield return typeof(DeleteDto);
            yield return typeof(DismountDto);
            yield return typeof(GetAllDto);
            yield return typeof(MountDto);
            yield return typeof(SetDefaultDto);
        }

        public override async Task<TCommandResult> Get<TCommandResult>(IProxyObjectWithOutput<TCommandResult> command, CancellationToken cancellationToken = default, IProgress<ProgressData> progress = null)
        {
            if (command is AddDto addDto)
            {
                object proxiedResult = await this.SelfElevationAwareObject.Add(addDto.DrivePath, cancellationToken, progress).ConfigureAwait(false);
                return (TCommandResult)proxiedResult;
            }

            if (command is GetAllDto)
            {
                object proxiedResult = await this.SelfElevationAwareObject.GetAll(cancellationToken, progress).ConfigureAwait(false);
                return (TCommandResult)proxiedResult;
            }

            throw new NotSupportedException();
        }

        public override Task Invoke(IProxyObject command, CancellationToken cancellationToken = default, IProgress<ProgressData> progress = null)
        {
            if (command is MovePackageToVolumeDto movePackageToVolumeDto)
            {
                return this.SelfElevationAwareObject.MovePackageToVolume(movePackageToVolumeDto.VolumePackagePath, movePackageToVolumeDto.PackageFullName, cancellationToken, progress);
            }

            if (command is DeleteDto deleteDto)
            {
                return this.SelfElevationAwareObject.Delete(deleteDto.Name, cancellationToken, progress);
            }

            if (command is DismountDto dismountDto)
            {
                return this.SelfElevationAwareObject.Dismount(dismountDto.Name, cancellationToken, progress);
            }

            if (command is MountDto mountDto)
            {
                return this.SelfElevationAwareObject.Mount(mountDto.Name, cancellationToken, progress);
            }

            if (command is SetDefaultDto setDefaultDto)
            {
                return this.SelfElevationAwareObject.SetDefault(setDefaultDto.DrivePath, cancellationToken, progress);
            }

            throw new NotSupportedException();
        }
    }
}
