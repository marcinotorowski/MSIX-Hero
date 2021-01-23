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

using System.Threading;
using Otor.MsixHero.App.Hero;
using Otor.MsixHero.App.Hero.Commands.Volumes;
using Otor.MsixHero.App.Hero.Events.Base;
using Otor.MsixHero.App.Hero.Executor;
using Otor.MsixHero.App.Mvvm;
using Otor.MsixHero.Infrastructure.Services;
using Otor.MsixHero.Lib.Infrastructure.Progress;

namespace Otor.MsixHero.App.Modules.VolumeManagement.ViewModels
{
    public class VolumesSearchViewModel : NotifyPropertyChanged
    {
        private readonly IMsixHeroApplication application;
        private readonly IBusyManager busyManager;
        private readonly IInteractionService interactionService;

        public VolumesSearchViewModel(
            IMsixHeroApplication application,
            IBusyManager busyManager,
            IInteractionService interactionService)
        {
            this.application = application;
            this.busyManager = busyManager;
            this.interactionService = interactionService;
            this.application.EventAggregator.GetEvent<UiExecutedEvent<SetVolumeFilterCommand>>().Subscribe(this.OnSetVolumeFilterCommand);
        }

        private void OnSetVolumeFilterCommand(UiExecutedPayload<SetVolumeFilterCommand> obj)
        {
            this.OnPropertyChanged(nameof(SearchKey));
        }


        public string SearchKey
        {
            get => this.application.ApplicationState.Volumes.SearchKey;
            set => this.application.CommandExecutor.Invoke(this, new SetVolumeFilterCommand(value));
        }
        
        private async void LoadContext()
        {
            var executor = this.application.CommandExecutor
                .WithBusyManager(this.busyManager, OperationType.VolumeLoading)
                .WithErrorHandling(this.interactionService, true);

            await executor.Invoke(this, new GetVolumesCommand(), CancellationToken.None).ConfigureAwait(false);
        }
    }
}
