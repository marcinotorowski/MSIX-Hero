﻿// MSIX Hero
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

using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Otor.MsixHero.App.Modules.Main.Events;
using Otor.MsixHero.App.Modules.SystemStatus.ViewModel.DeveloperMode;
using Otor.MsixHero.App.Modules.SystemStatus.ViewModel.Repackaging;
using Otor.MsixHero.App.Modules.SystemStatus.ViewModel.Tooling;
using Otor.MsixHero.App.Modules.SystemStatus.ViewModel.WindowsStoreUpdates;
using Otor.MsixHero.App.Mvvm;
using Otor.MsixHero.Appx.Diagnostic.Developer;
using Otor.MsixHero.Appx.Diagnostic.Recommendations;
using Otor.MsixHero.Appx.Diagnostic.Recommendations.ThirdParty;
using Otor.MsixHero.Appx.Diagnostic.Store;
using Otor.MsixHero.Infrastructure.Services;
using Prism.Events;
using Prism.Navigation.Regions;

namespace Otor.MsixHero.App.Modules.SystemStatus.ViewModel
{
    public class SystemStatusViewModel : NotifyPropertyChanged, INavigationAware
    {
        protected readonly ISideloadingConfigurator SideloadingConfigurator = new SideloadingConfigurator();
        protected readonly IWindowsStoreAutoDownloadConfigurator WindowsStoreAutoDownloadConfigurator = new WindowsStoreAutoDownloadConfigurator();
        private readonly IEventAggregator _eventAggregator;
        private bool _isLoading;

        public SystemStatusViewModel(
            IEventAggregator eventAggregator,
            IThirdPartyAppProvider thirdPartyDetector,
            IServiceRecommendationAdvisor serviceAdvisor,
            IInteractionService interactionService)
        {
            this._eventAggregator = eventAggregator;
            this.Items = new ObservableCollection<BaseRecommendationViewModel>();

            if (this.SideloadingConfigurator.Flavor == SideloadingFlavor.Windows10Below2004)
            {
                var sideLoadingAndDeveloperMode = new DeveloperAndSideloadingRecommendationViewModel(this.SideloadingConfigurator);
                this.Items.Add(sideLoadingAndDeveloperMode);
            }
            else
            {
                var sideLoading = new SideloadingRecommendationViewModel();
                this.Items.Add(sideLoading);

                var developer = new DeveloperModeRecommendationViewModel(this.SideloadingConfigurator);
                this.Items.Add(developer);
            }

            var storeAutoDownload = new AutoDownloadRecommendationViewModel(this.WindowsStoreAutoDownloadConfigurator);
            var repackaging = new RepackagingRecommendationViewModel(serviceAdvisor, interactionService, storeAutoDownload);
            var tooling = new ToolingRecommendationViewModel(thirdPartyDetector);

            this.Items.Add(storeAutoDownload);
            this.Items.Add(repackaging);
            this.Items.Add(tooling);
        }

        public SystemStatusCommandHandler CommandHandler { get; } = new SystemStatusCommandHandler();

        public ObservableCollection<BaseRecommendationViewModel> Items { get; }
        public bool IsLoading
        {
            get => this._isLoading;
            set => this.SetField(ref this._isLoading, value);
        }

        bool IRegionAware.IsNavigationTarget(NavigationContext navigationContext)
        {
            return true;
        }

        void IRegionAware.OnNavigatedFrom(NavigationContext navigationContext)
        {
        }

        void IRegionAware.OnNavigatedTo(NavigationContext navigationContext)
        {
            this._eventAggregator.GetEvent<TopSearchWidthChangeEvent>().Publish(new TopSearchWidthChangeEventPayLoad(0.0));
            this.Refresh();
        }

        public async void Refresh()
        {
#pragma warning disable 4014
            this.IsLoading = true;
            await Task.Delay(400).ConfigureAwait(false);
            try
            {
                var allTasks = this.Items.Select(t => t.Refresh());
                await Task.WhenAll(allTasks).ConfigureAwait(true);
            }
            finally
            {
                this.IsLoading = false;
            }
#pragma warning restore 4014
        }
    }
}
