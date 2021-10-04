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
    public class AppxPackageInstallerProxyReceiver : SelfElevationProxyReceiver<IAppxPackageInstaller>
    {
        public AppxPackageInstallerProxyReceiver(IAppxPackageInstaller selfElevationAware) : base(selfElevationAware)
        {
        }

        public override IEnumerable<Type> GetSupportedProxiedObjectTypes()
        {
            yield return typeof(AddDto);
            yield return typeof(DeprovisionDto);
            yield return typeof(GetModificationPackagesDto);
            yield return typeof(CheckIfInstalledDto);
            yield return typeof(RemoveDto);
            yield return typeof(RemoveCurrentUserDto);
        }

        public override async Task<TCommandResult> Get<TCommandResult>(IProxyObjectWithOutput<TCommandResult> command, CancellationToken cancellationToken = default, IProgress<ProgressData> progress = null)
        {
            switch (command)
            {
                case CheckIfInstalledDto checkIfInstalledDto:
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

                default:
                {
                    throw new NotSupportedException();
                }
            }
        }

        public override Task Invoke(IProxyObject command, CancellationToken cancellationToken = default, IProgress<ProgressData> progress = null)
        {
            switch (command)
            {
                case AddDto addDto:
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
                case DeprovisionDto deprovisionDto:
                {
                    return this.SelfElevationAwareObject.Deprovision(deprovisionDto.PackageFamilyName, cancellationToken, progress);
                }

                case RemoveDto removeDto:
                {
                    return this.SelfElevationAwareObject.Remove(removeDto.Packages, removeDto.Context == PackageContext.AllUsers, !removeDto.RemoveAppData, cancellationToken, progress);
                }

                case RemoveCurrentUserDto removeCurrentUserDto:
                {
                    return this.SelfElevationAwareObject.Remove(removeCurrentUserDto.PackageFullNames, !removeCurrentUserDto.RemoveAppData, cancellationToken, progress);
                }

                default:
                {
                    throw new NotSupportedException();
                }
            }
        }
    }
}