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
using Otor.MsixHero.Appx.Packaging.Installation.Enums;
using Otor.MsixHero.Infrastructure.Processes.Ipc;
using Otor.MsixHero.Infrastructure.Processes.SelfElevation.Enums;
using Otor.MsixHero.Infrastructure.Progress;
using Otor.MsixHero.Lib.Proxy.Packaging.Dto;

namespace Otor.MsixHero.Lib.Proxy.Packaging
{
    public class AppxPackageQueryProxyReceiver : SelfElevationProxyReceiver<IAppxPackageQuery>
    {
        public AppxPackageQueryProxyReceiver(IAppxPackageQuery selfElevationAware) : base(selfElevationAware)
        {
        }


        public override IEnumerable<Type> GetSupportedProxiedObjectTypes()
        {
            yield return typeof(GetByManifestPathDto);
            yield return typeof(GetByIdentityDto);
            yield return typeof(GetInstalledPackagesDto);
            yield return typeof(GetModificationPackagesDto);
            yield return typeof(GetUsersForPackageDto);
        }

        public override async Task<TCommandResult> Get<TCommandResult>(IProxyObjectWithOutput<TCommandResult> command, CancellationToken cancellationToken = default, IProgress<ProgressData> progress = null)
        {
            switch (command)
            {
                case GetByManifestPathDto getByManifestPathDto:
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

                case GetByIdentityDto getByIdentityDto:
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

                case GetInstalledPackagesDto getInstalledPackagesDto:
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

                case GetModificationPackagesDto getModificationPackagesDto:
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

                case GetUsersForPackageDto getUsersForPackageDto:
                {
                    object proxiedObject = await this.SelfElevationAwareObject.GetUsersForPackage(getUsersForPackageDto.Source, cancellationToken, progress).ConfigureAwait(false);
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
            throw new NotSupportedException();
        }
    }
}