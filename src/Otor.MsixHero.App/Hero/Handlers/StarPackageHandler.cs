// MSIX Hero
// Copyright (C) 2024 Marcin Otorowski
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
using MediatR;
using Otor.MsixHero.App.Hero.Commands.Packages;
using Otor.MsixHero.Appx.Packaging;
using Otor.MsixHero.Infrastructure.Configuration;
using Otor.MsixHero.Infrastructure.Services;

namespace Otor.MsixHero.App.Hero.Handlers;

public class StarPackageHandler : IRequestHandler<StarPackageCommand>
{
    private readonly IConfigurationService _configurationService;

    public StarPackageHandler(IConfigurationService configurationService)
    {
        _configurationService = configurationService;
    }

    async Task IRequestHandler<StarPackageCommand>.Handle(StarPackageCommand request, CancellationToken cancellationToken)
    {
        var config = await this._configurationService.GetCurrentConfigurationAsync(false, cancellationToken).ConfigureAwait(false);
        if (config.Packages?.StarredApps?.Contains(request.FullName) == true)
        {
            return;
        }

        if (config.Packages == null)
        {
            config.Packages = new PackagesConfiguration();
        }

        if (config.Packages.StarredApps == null)
        {
            config.Packages.StarredApps = new List<string>();
        }
        
        if (request.AnyVersion)
        {
            if (!PackageIdentity.TryFromFullName(request.FullName, out var lookFor))
            {
                // requested string does not seem like a full name, so ignore it.
                return;
            }

            lookFor.AppVersion = new Version(0, 0, 0, 0);
            
            // Clean-up anything that matches the package
            for (var i = config.Packages.StarredApps.Count - 1; i >= 0; i--)
            {
                if (!PackageIdentity.TryFromFullName(config.Packages.StarredApps[i], out var current))
                {
                    // do not replicate errors, invalid string is invalid.
                    config.Packages.StarredApps.RemoveAt(i);
                    continue;
                }
                
                if (lookFor.AppName == current.AppName &&
                    lookFor.PublisherHash == current.PublisherHash &&
                    lookFor.ResourceId == current.ResourceId &&
                    lookFor.Architecture == current.Architecture)
                {
                    config.Packages.StarredApps.RemoveAt(i);
                }
            }

            config.Packages.StarredApps.Add(lookFor.ToString());
        }
        else
        {
            config.Packages.StarredApps.Add(request.FullName);
        }

        await this._configurationService.SetCurrentConfigurationAsync(config, cancellationToken).ConfigureAwait(false);
    }
}