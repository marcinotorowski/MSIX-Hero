// MSIX Hero
// Copyright (C) 2021 Marcin Otorowski
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// Full notice:
// https://github.com/marcinotorowski/msix-hero/blob/develop/LICENSE.md

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Otor.MsixHero.Appx.Packaging.Installation;
using Otor.MsixHero.Appx.Packaging.Installation.Entities;
using Otor.MsixHero.Appx.Packaging.Installation.Enums;
using Otor.MsixHero.Infrastructure.Processes.Ipc;
using Otor.MsixHero.Infrastructure.Processes.SelfElevation.Enums;
using Otor.MsixHero.Infrastructure.Progress;
using Otor.MsixHero.Lib.Proxy.Packaging.Dto;

namespace Otor.MsixHero.Lib.Proxy.Packaging
{
    public class AppxPackageManagerProxyReceiver : SelfElevationProxyReceiver<IAppxPackageManager>
    {
        public AppxPackageManagerProxyReceiver(IAppxPackageManager selfElevationAware) : base(selfElevationAware)
        {
        }

        public override IEnumerable<Type> GetSupportedProxiedObjectTypes()
        {
            yield return typeof(AddDto);
            yield return typeof(DeprovisionDto);
            yield return typeof(CheckUpdateAvailabilityDto);
            yield return typeof(StopDto);
            yield return typeof(GetByManifestPathDto);
            yield return typeof(GetByIdentityDto);
            yield return typeof(GetInstalledPackagesDto);
            yield return typeof(CheckIfInstalledDto);
            yield return typeof(GetModificationPackagesDto);
            yield return typeof(GetUsersForPackageDto);
            yield return typeof(RemoveDto);
            yield return typeof(RemoveCurrentUserDto);
            yield return typeof(RunDto);
            yield return typeof(RunToolInContextDto);
        }

        public override async Task<TCommandResult> Get<TCommandResult>(IProxyObjectWithOutput<TCommandResult> command, CancellationToken cancellationToken = default, IProgress<ProgressData> progress = null)
        {
            if (command is GetByManifestPathDto getByManifestPathDto)
            {
                // todo: replace by simple enum
                PackageFindMode mode;
                switch (getByManifestPathDto.Context)
                {
                    case PackageContext.CurrentUser:
                        mode = PackageFindMode.CurrentUser;
                        break;
                    case PackageContext.AllUsers:
                        mode = PackageFindMode.AllUsers;
                        break;
                    default:
                        mode = PackageFindMode.Auto;
                        break;
                }

                object proxiedObject = await this.SelfElevationAwareObject.GetByIdentity(getByManifestPathDto.Source, mode, cancellationToken, progress).ConfigureAwait(false);
                return (TCommandResult)proxiedObject;
            }

            if (command is GetByIdentityDto getByIdentityDto)
            {
                // todo: replace by simple enum
                PackageFindMode mode;
                switch (getByIdentityDto.Context)
                {
                    case PackageContext.CurrentUser:
                        mode = PackageFindMode.CurrentUser;
                        break;
                    case PackageContext.AllUsers:
                        mode = PackageFindMode.AllUsers;
                        break;
                    default:
                        mode = PackageFindMode.Auto;
                        break;
                }

                object proxiedObject = await this.SelfElevationAwareObject.GetByIdentity(getByIdentityDto.Source, mode, cancellationToken, progress).ConfigureAwait(false);
                return (TCommandResult)proxiedObject;
            }

            if (command is CheckIfInstalledDto checkIfInstalledDto)
            {
                // todo: replace by simple enum
                PackageFindMode mode;
                switch (checkIfInstalledDto.Context)
                {
                    case PackageContext.CurrentUser:
                        mode = PackageFindMode.CurrentUser;
                        break;
                    case PackageContext.AllUsers:
                        mode = PackageFindMode.AllUsers;
                        break;
                    default:
                        mode = PackageFindMode.Auto;
                        break;
                }

                object proxiedObject = await this.SelfElevationAwareObject.IsInstalled(checkIfInstalledDto.ManifestFilePath, mode, cancellationToken, progress).ConfigureAwait(false);
                return (TCommandResult)proxiedObject;
            }

            if (command is GetInstalledPackagesDto getInstalledPackagesDto)
            {
                // todo: replace by simple enum
                PackageFindMode mode;
                switch (getInstalledPackagesDto.Context)
                {
                    case PackageContext.CurrentUser:
                        mode = PackageFindMode.CurrentUser;
                        break;
                    case PackageContext.AllUsers:
                        mode = PackageFindMode.AllUsers;
                        break;
                    default:
                        mode = PackageFindMode.Auto;
                        break;
                }

                object proxiedObject = await this.SelfElevationAwareObject.GetInstalledPackages(mode, cancellationToken, progress).ConfigureAwait(false);
                return (TCommandResult)proxiedObject;
            }

            if (command is GetModificationPackagesDto getModificationPackagesDto)
            {
                // todo: replace by simple enum
                PackageFindMode mode;
                switch (getModificationPackagesDto.Context)
                {
                    case PackageContext.CurrentUser:
                        mode = PackageFindMode.CurrentUser;
                        break;
                    case PackageContext.AllUsers:
                        mode = PackageFindMode.AllUsers;
                        break;
                    default:
                        mode = PackageFindMode.Auto;
                        break;
                }

                object proxiedObject = await this.SelfElevationAwareObject.GetModificationPackages(getModificationPackagesDto.FullPackageName, mode, cancellationToken, progress).ConfigureAwait(false);
                return (TCommandResult)proxiedObject;
            }

            if (command is GetUsersForPackageDto getUsersForPackageDto)
            {
                object proxiedObject = await this.SelfElevationAwareObject.GetUsersForPackage(getUsersForPackageDto.Source, cancellationToken, progress).ConfigureAwait(false);
                return (TCommandResult)proxiedObject;
            }

            throw new NotSupportedException();
        }

        public override Task Invoke(IProxyObject command, CancellationToken cancellationToken = default, IProgress<ProgressData> progress = null)
        {
            if (command is AddDto addDto)
            {
                AddAppxPackageOptions addDtoOptions = 0;

                if (addDto.AllUsers)
                {
                    addDtoOptions |= AddAppxPackageOptions.AllUsers;
                }

                if (addDto.AllowDowngrade)
                {
                    addDtoOptions |= AddAppxPackageOptions.AllowDowngrade;
                }

                if (addDto.KillRunningApps)
                {
                    addDtoOptions |= AddAppxPackageOptions.KillRunningApps;
                }

                return this.SelfElevationAwareObject.Add(addDto.FilePath, addDtoOptions, cancellationToken, progress);
            }

            if (command is DeprovisionDto deprovisionDto)
            {
                return this.SelfElevationAwareObject.Deprovision(deprovisionDto.PackageFamilyName, cancellationToken, progress);
            }

            if (command is CheckUpdateAvailabilityDto checkUpdatesDto)
            {
                return this.SelfElevationAwareObject.CheckForUpdates(checkUpdatesDto.PackageFullName, cancellationToken, progress);
            }

            if (command is StopDto stopDto)
            {
                return this.SelfElevationAwareObject.Stop(stopDto.PackageFullName, cancellationToken);
            }

            if (command is RemoveDto removeDto)
            {
                return this.SelfElevationAwareObject.Remove(removeDto.Packages, removeDto.Context == PackageContext.AllUsers, !removeDto.RemoveAppData, cancellationToken, progress);
            }

            if (command is RemoveCurrentUserDto removeCurrentUserDto)
            {
                return this.SelfElevationAwareObject.Remove(removeCurrentUserDto.PackageFullNames, !removeCurrentUserDto.RemoveAppData, cancellationToken, progress);
            }

            if (command is RunDto runPackageDto)
            {
                return this.SelfElevationAwareObject.Run(runPackageDto.ManifestPath, runPackageDto.ApplicationId, cancellationToken, progress);
            }

            if (command is RunToolInContextDto runToolInContextDto)
            {
                return this.SelfElevationAwareObject.RunToolInContext(runToolInContextDto.PackageFamilyName, runToolInContextDto.AppId, runToolInContextDto.ToolPath, runToolInContextDto.Arguments, cancellationToken, progress);
            }

            throw new NotSupportedException();
        }
    }
}
