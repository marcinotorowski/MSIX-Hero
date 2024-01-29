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
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Data;
using Otor.MsixHero.App.Hero;
using Otor.MsixHero.App.Hero.Commands.Containers;
using Otor.MsixHero.App.Hero.Events.Base;
using Otor.MsixHero.App.Hero.Executor;
using Otor.MsixHero.App.Mvvm;
using Otor.MsixHero.App.Mvvm.Progress;
using Otor.MsixHero.Appx.Packaging.SharedPackageContainer.Entities;
using Otor.MsixHero.Infrastructure.Configuration;
using Otor.MsixHero.Infrastructure.Services;
using Prism;
using Prism.Events;

namespace Otor.MsixHero.App.Modules.Containers.List.ViewModels
{
    public class ContainersListViewModel : NotifyPropertyChanged, IActiveAware
    {
        private readonly IMsixHeroApplication _application;
        private readonly IBusyManager _busyManager;
        private readonly IInteractionService _interactionService;
        private bool _firstRun = true;
        private bool _isActive;

        public ContainersListViewModel(
            IMsixHeroApplication application,
            IBusyManager busyManager,
            IInteractionService interactionService)
        {
            this._application = application;
            this._busyManager = busyManager;
            this._interactionService = interactionService;
            this.Containers = new ObservableCollection<SharedPackageContainerViewModel>();
            this.ContainersView = CollectionViewSource.GetDefaultView(this.Containers);
            this.ContainersView.Filter += Filter;
            this.Sort(nameof(SharedPackageContainerViewModel.Name), false);

            this._busyManager.StatusChanged += BusyManagerOnStatusChanged;
            this.Progress = new ProgressProperty();
            this._application.EventAggregator.GetEvent<UiExecutedEvent<GetSharedPackageContainersCommand, IList<SharedPackageContainer>>>().Subscribe(this.OnGet, ThreadOption.UIThread);
            this._application.EventAggregator.GetEvent<UiExecutedEvent<SetSharedPackageContainersSortingCommand>>().Subscribe(this.OnSetSorting, ThreadOption.UIThread);
            this._application.EventAggregator.GetEvent<UiExecutedEvent<SetSharedPackageContainersFilterCommand>>().Subscribe(this.OnSetFilterCommand, ThreadOption.UIThread);
            this.SetSortingAndGrouping();
        }

        private void SetSortingAndGrouping()
        {
            var currentSort = this._application.ApplicationState.Containers.SortMode;
            var currentSortDescending = this._application.ApplicationState.Containers.SortDescending;

            using (this.ContainersView.DeferRefresh())
            {
                string sortProperty;

                switch (currentSort)
                {
                    case ContainerSort.Name:
                        sortProperty = nameof(SharedPackageContainerViewModel.Name);
                        break;
                    default:
                        sortProperty = null;
                        break;
                }

                if (sortProperty == null)
                {
                    this.ContainersView.SortDescriptions.Clear();
                }
                else
                {
                    var sd = this.ContainersView.SortDescriptions.FirstOrDefault();
                    if (sd.PropertyName != sortProperty || sd.Direction != (currentSortDescending ? ListSortDirection.Descending : ListSortDirection.Ascending))
                    {
                        this.ContainersView.SortDescriptions.Clear();
                        this.ContainersView.SortDescriptions.Add(new SortDescription(sortProperty, currentSortDescending ? ListSortDirection.Descending : ListSortDirection.Ascending));
                    }
                }

                if (this.ContainersView.GroupDescriptions.Any())
                {
                    var gpn = ((PropertyGroupDescription)this.ContainersView.GroupDescriptions[0]).PropertyName;
                    if (this.ContainersView.GroupDescriptions.Any() && this.ContainersView.SortDescriptions.All(sd => sd.PropertyName != gpn))
                    {
                        this.ContainersView.SortDescriptions.Insert(0, new SortDescription(gpn, ListSortDirection.Ascending));
                    }
                }
            }
        }
        private void BusyManagerOnStatusChanged(object sender, IBusyStatusChange e)
        {
            if (e.Type != OperationType.ContainerLoading)
            {
                return;
            }

            this.Progress.Progress = e.Progress;
            this.Progress.Message = e.Message;
            this.Progress.IsLoading = e.IsBusy;
        }

        public ProgressProperty Progress { get; }
        
        private void OnSetSorting(UiExecutedPayload<SetSharedPackageContainersSortingCommand> _)
        {
            this.SetSortingAndGrouping();
        }

        private void OnGet(UiExecutedPayload<GetSharedPackageContainersCommand, IList<SharedPackageContainer>> obj)
        {
            this.Containers.Clear();
            
            foreach (var item in obj.Result)
            {
                this.Containers.Add(new SharedPackageContainerViewModel(item));
            }
        }
        
        public string SearchKey
        {
            get => this._application.ApplicationState.Containers.SearchKey;
            set => this._application.CommandExecutor.Invoke(this, new SetSharedPackageContainersFilterCommand(value));
        }
        
        public ObservableCollection<SharedPackageContainerViewModel> Containers { get; }

        public ICollectionView ContainersView { get; }
        
        public void Sort(string columnName, bool descending)
        {
            this.ContainersView.SortDescriptions.Clear();
            this.ContainersView.SortDescriptions.Add(new SortDescription(columnName, descending ? ListSortDirection.Descending : ListSortDirection.Ascending));
        }

        private bool Filter(object obj)
        {
            var filtered = (SharedPackageContainerViewModel)obj;
            
            var searchKey = this._application.ApplicationState.Containers.SearchKey;
            if (!string.IsNullOrEmpty(searchKey))
            {
                return filtered.Name.IndexOf(searchKey, StringComparison.OrdinalIgnoreCase) != -1;
            }
            
            return true;
        }

        public bool IsActive
        {
            get => this._isActive;
            set
            {
                if (this._isActive == value)
                {
                    return;
                }

                this._isActive = value;

                this.IsActiveChanged?.Invoke(this, EventArgs.Empty);

                if (value && this._firstRun)
                {
#pragma warning disable 4014
                    this.SetInitialData();
#pragma warning restore 4014
                }
            }
        }

        public event EventHandler IsActiveChanged;

        private async Task SetInitialData()
        {
            this._firstRun = false;
            using var cts = new CancellationTokenSource();
            using var task = this._application.CommandExecutor
                .WithBusyManager(this._busyManager, OperationType.ContainerLoading)
                .WithErrorHandling(this._interactionService, true)
                .Invoke<GetSharedPackageContainersCommand, IList<SharedPackageContainer>>(this, new GetSharedPackageContainersCommand(), cts.Token);

            this.Progress.MonitorProgress(task, cts);
            await task.ConfigureAwait(false);
        }

        private void OnSetFilterCommand(UiExecutedPayload<SetSharedPackageContainersFilterCommand> _)
        {
            this.OnPropertyChanged(nameof(this.SearchKey));
        }
    }
}

