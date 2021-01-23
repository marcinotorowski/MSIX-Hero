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

using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Otor.MsixHero.App.Events;
using Otor.MsixHero.App.Modules.SystemStatus.ViewModel.DeveloperMode;
using Otor.MsixHero.App.Modules.SystemStatus.ViewModel.Repackaging;
using Otor.MsixHero.App.Modules.SystemStatus.ViewModel.Tooling;
using Otor.MsixHero.App.Modules.SystemStatus.ViewModel.WindowsStoreUpdates;
using Otor.MsixHero.App.Mvvm;
using Otor.MsixHero.Appx.Diagnostic.Developer;
using Otor.MsixHero.Appx.Diagnostic.Recommendations;
using Otor.MsixHero.Appx.Diagnostic.Recommendations.ThirdParty;
using Otor.MsixHero.Infrastructure.Services;
using Prism.Events;
using Prism.Regions;

namespace Otor.MsixHero.App.Modules.SystemStatus.ViewModel
{
    public class SystemStatusViewModel : NotifyPropertyChanged, INavigationAware
    {
        protected readonly ISideloadingChecker SideLoadCheck = new RegistrySideloadingChecker();
        private readonly IEventAggregator eventAggregator;
        private bool isLoading;

        public SystemStatusViewModel(
            IEventAggregator eventAggregator,
            IThirdPartyAppProvider thirdPartyDetector,
            IServiceRecommendationAdvisor serviceAdvisor,
            IInteractionService interactionService)
        {
            this.eventAggregator = eventAggregator;
            this.Items = new ObservableCollection<BaseRecommendationViewModel>();

            var sideloading = new DeveloperAndSideloadingRecommendationViewModel(this.SideLoadCheck);
            var storeAutoDownload = new AutoDownloadRecommendationViewModel(this.SideLoadCheck);
            var repackaging = new RepackagingRecommendationViewModel(serviceAdvisor, interactionService, storeAutoDownload);
            var tooling = new ToolingRecommendationViewModel(thirdPartyDetector);

            this.Items.Add(sideloading);
            this.Items.Add(storeAutoDownload);
            this.Items.Add(repackaging);
            this.Items.Add(tooling);
        }

        public SystemStatusCommandHandler CommandHandler { get; } = new SystemStatusCommandHandler();

        public ObservableCollection<BaseRecommendationViewModel> Items { get; }
        public bool IsLoading
        {
            get => this.isLoading;
            set => this.SetField(ref this.isLoading, value);
        }

        bool INavigationAware.IsNavigationTarget(NavigationContext navigationContext)
        {
            return true;
        }

        void INavigationAware.OnNavigatedFrom(NavigationContext navigationContext)
        {
        }

        void INavigationAware.OnNavigatedTo(NavigationContext navigationContext)
        {
            this.eventAggregator.GetEvent<TopSearchWidthChangeEvent>().Publish(new TopSearchWidthChangeEventPayLoad(0.0));
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
