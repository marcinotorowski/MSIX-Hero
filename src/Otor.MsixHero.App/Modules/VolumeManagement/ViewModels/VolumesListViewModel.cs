// MSIX Hero
// Copyright (C) 2022 Marcin Otorowski
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
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Data;
using Otor.MsixHero.App.Helpers;
using Otor.MsixHero.App.Hero;
using Otor.MsixHero.App.Hero.Commands.Volumes;
using Otor.MsixHero.App.Hero.Events.Base;
using Otor.MsixHero.App.Hero.Executor;
using Otor.MsixHero.App.Mvvm;
using Otor.MsixHero.Appx.Volumes.Entities;
using Otor.MsixHero.Infrastructure.Services;
using Otor.MsixHero.Lib.Infrastructure.Progress;
using Prism;
using Prism.Events;

namespace Otor.MsixHero.App.Modules.VolumeManagement.ViewModels
{
    public class VolumesListViewModel : NotifyPropertyChanged, IActiveAware
    {
        private readonly IBusyManager busyManager;
        private readonly IMsixHeroApplication application;
        private readonly IInteractionService interactionService;
        private bool isActive;
        private bool firstRun = true;

        public VolumesListViewModel(
            IMsixHeroApplication application, 
            IBusyManager busyManager,
            IInteractionService interactionService)
        {
            this.busyManager = busyManager;
            this.application = application;
            this.interactionService = interactionService;
            this.Items = new ObservableCollection<VolumeViewModel>();
            this.ItemsCollection = CollectionViewSource.GetDefaultView(this.Items);
            this.ItemsCollection.Filter = row => this.IsVolumeVisible((VolumeViewModel)row);

            this.application.EventAggregator.GetEvent<UiExecutingEvent<GetVolumesCommand>>().Subscribe(this.OnGetVolumesExecuting);
            this.application.EventAggregator.GetEvent<UiExecutedEvent<GetVolumesCommand, IList<AppxVolume>>>().Subscribe(this.OnGetVolumesExecuted, ThreadOption.UIThread);
            this.application.EventAggregator.GetEvent<UiFailedEvent<GetVolumesCommand>>().Subscribe(this.OnGetVolumesFailed);
            this.application.EventAggregator.GetEvent<UiCancelledEvent<GetVolumesCommand>>().Subscribe(this.OnGetVolumesCancelled);

            // filtering
            this.application.EventAggregator.GetEvent<UiExecutedEvent<SetVolumeFilterCommand>>().Subscribe(this.OnSetVolumeFilterCommand, ThreadOption.UIThread);

            this.busyManager.StatusChanged += BusyManagerOnStatusChanged;
        }

        public bool IsActive
        {
            get => this.isActive;
            set
            {
                if (!this.SetField(ref this.isActive, value))
                {
                    return;
                }

                this.IsActiveChanged?.Invoke(this, EventArgs.Empty);

                if (this.firstRun)
                {
                    this.application
                        .CommandExecutor
                        .WithBusyManager(this.busyManager, OperationType.VolumeLoading)
                        .WithErrorHandling(this.interactionService, true)
                        .Invoke<GetVolumesCommand, IList<AppxVolume>>(this, new GetVolumesCommand());
                }

                this.firstRun = false;
            }
        }

        public event EventHandler IsActiveChanged;
        
        public ICollectionView ItemsCollection { get; }

        public ObservableCollection<VolumeViewModel> Items { get; }

        public ProgressProperty Progress { get; } = new ProgressProperty();

        public string SearchKey
        {
            get => this.application.ApplicationState.Volumes.SearchKey;
            private set => this.application.CommandExecutor.Invoke(this, new SetVolumeFilterCommand(value));
        }

        private void BusyManagerOnStatusChanged(object sender, IBusyStatusChange e)
        {
            if (e.Type != OperationType.VolumeLoading && e.Type != OperationType.Other)
            {
                return;
            }

            this.Progress.IsLoading = e.IsBusy;
            this.Progress.Message = e.Message;
            this.Progress.Progress = e.Progress;
        }

        private void OnGetVolumesExecuting(UiExecutingPayload<GetVolumesCommand> eventPayload)
        {
        }

        private void OnGetVolumesCancelled(UiCancelledPayload<GetVolumesCommand> eventPayload)
        {
        }

        private void OnGetVolumesFailed(UiFailedPayload<GetVolumesCommand> eventPayload)
        {
        }

        private void OnGetVolumesExecuted(UiExecutedPayload<GetVolumesCommand, IList<AppxVolume>> eventPayload)
        {
            this.Items.Clear();

            foreach (var item in eventPayload.Result)
            {
                this.Items.Add(new VolumeViewModel(item));
            }
        }

        private bool IsVolumeVisible(VolumeViewModel volume)
        {
            if (string.IsNullOrWhiteSpace(this.SearchKey))
            {
                return true;
            }

            if (
                (volume.Name != null && volume.Name.IndexOf(this.SearchKey, StringComparison.OrdinalIgnoreCase) > -1) ||
                (volume.PackageStorePath != null &&
                 volume.PackageStorePath.IndexOf(this.SearchKey, StringComparison.OrdinalIgnoreCase) > -1) ||
                (volume.Label != null && volume.Label.IndexOf(this.SearchKey, StringComparison.OrdinalIgnoreCase) > -1))
            {
                return true;
            }

            return false;
        }

        private void OnSetVolumeFilterCommand(UiExecutedPayload<SetVolumeFilterCommand> obj)
        {
            this.OnPropertyChanged(nameof(SearchKey));
            this.ItemsCollection.Refresh();
        }
    }
}
