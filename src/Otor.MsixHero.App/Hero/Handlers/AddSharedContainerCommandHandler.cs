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
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Otor.MsixHero.App.Hero.Commands.Containers;
using Otor.MsixHero.App.Hero.Executor;
using Otor.MsixHero.App.Mvvm.Progress;
using Otor.MsixHero.Appx.Packaging.SharedPackageContainer;
using Otor.MsixHero.Appx.Packaging.SharedPackageContainer.Entities;
using Otor.MsixHero.Elevation;
using Otor.MsixHero.Infrastructure.Progress;

namespace Otor.MsixHero.App.Hero.Handlers;

public class AddSharedContainerCommandHandler : IRequestHandler<AddSharedContainerCommand, SharedPackageContainer>
{
    private readonly IUacElevation _uacElevation;
    private readonly IAppxSharedPackageContainerService _containerService;
    private readonly IMsixHeroCommandExecutor _commandExecutor;
    private readonly IBusyManager _busyManager;

    public AddSharedContainerCommandHandler(
        IMsixHeroCommandExecutor commandExecutor,
        IBusyManager busyManager,
        IUacElevation uacElevation,
        IAppxSharedPackageContainerService containerService)
    {
        this._commandExecutor = commandExecutor;
        this._busyManager = busyManager;
        this._uacElevation = uacElevation;
        this._containerService = containerService;
    }

    public async Task<SharedPackageContainer> Handle(AddSharedContainerCommand request, CancellationToken cancellationToken)
    {
        var context = this._busyManager.Begin(OperationType.ContainerLoading);
        try
        {
            using var wrappedProgress = new WrappedProgress(context);
            var p1 = wrappedProgress.GetChildProgress(0.8);
            var p2 = wrappedProgress.GetChildProgress(0.2);

            p1.Report(new ProgressData(0, Resources.Localization.Containers_PleaseWait_Add));
            
            // Create new container
            var uacProxy =
                this._containerService.IsAdminRequiredToManage()
                    ? this._uacElevation.AsAdministrator<IAppxSharedPackageContainerService>()
                    : this._uacElevation.AsCurrentUser<IAppxSharedPackageContainerService>();

            var newContainer = await uacProxy.Add(request.Container, request.ForceApplicationShutdown, request.ConflictResolution, cancellationToken).ConfigureAwait(false);
            
            p1.Report(new ProgressData(100, Resources.Localization.Containers_PleaseWait));

            // Force refresh of containers
            var allContainers = await this._commandExecutor.Invoke<GetSharedPackageContainersCommand, IList<SharedPackageContainer>>(this, new GetSharedPackageContainersCommand(), cancellationToken, p2).ConfigureAwait(false);

            p2.Report(new ProgressData(100, Resources.Localization.Containers_PleaseWait));

            return allContainers.FirstOrDefault(c => string.Equals(c.Name, newContainer.Name, StringComparison.OrdinalIgnoreCase));
        }
        finally
        {
            this._busyManager.End(context);
        }
    }
}