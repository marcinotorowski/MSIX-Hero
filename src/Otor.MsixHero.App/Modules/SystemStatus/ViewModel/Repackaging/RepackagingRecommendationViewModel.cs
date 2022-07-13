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

using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media;
using Otor.MsixHero.App.Modules.SystemStatus.ViewModel.WindowsStoreUpdates;
using Otor.MsixHero.Appx.Diagnostic.Developer.Enums;
using Otor.MsixHero.Appx.Diagnostic.Recommendations;
using Otor.MsixHero.Appx.Diagnostic.Recommendations.Enums;
using Otor.MsixHero.Infrastructure.Localization;
using Otor.MsixHero.Infrastructure.Services;

namespace Otor.MsixHero.App.Modules.SystemStatus.ViewModel.Repackaging
{
    public class RepackagingRecommendationViewModel : BaseRecommendationViewModel
    {
        protected readonly IServiceRecommendationAdvisor ServiceAdvisor;
        private readonly IInteractionService interactionService;

        public RepackagingRecommendationViewModel(
            IServiceRecommendationAdvisor serviceAdvisor,
            IInteractionService interactionService,
            AutoDownloadRecommendationViewModel autoDownloadRecommendation)
        {
            this.ServiceAdvisor = serviceAdvisor;
            this.interactionService = interactionService;
            this.AutoDownloadRecommendation = autoDownloadRecommendation;
            this.Items = new ObservableCollection<ServiceRecommendationViewModel>();
            this.AutoDownloadRecommendation.PropertyChanged += this.AutoDownloadRecommendationOnPropertyChanged;
        }

        private void AutoDownloadRecommendationOnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == null || e.PropertyName == nameof(AutoDownloadRecommendationViewModel.AutoDownloadStatus))
            {
#pragma warning disable 4014
                this.Refresh();
#pragma warning restore 4014
            }
        }

        public AutoDownloadRecommendationViewModel AutoDownloadRecommendation { get; }

        public override string Title { get; } = Resources.Localization.System_Repackaging;

        public override async Task Refresh(CancellationToken cancellationToken = default)
        {
            this.IsLoading = true;
            try
            {
                await Task.Delay(1200, cancellationToken).ConfigureAwait(false);
                var list = await Task.Run(() => this.ServiceAdvisor.Advise(AdvisorMode.ForPackaging).ToList(), cancellationToken).ConfigureAwait(true);

                var newItems = new ObservableCollection<ServiceRecommendationViewModel>();
                foreach (var item in list)
                {
                    newItems.Add(new ServiceRecommendationViewModel(this, this.ServiceAdvisor, item, this.interactionService));
                }

                this.Items = newItems;
                this.OnPropertyChanged(nameof(Items));

                var suggestedActions = this.Items.Count(c => c.IsRunning != c.ShouldRun);

                if (this.AutoDownloadRecommendation.AutoDownloadStatus != WindowsStoreAutoDownload.Never)
                {
                    suggestedActions++;
                }

                switch (suggestedActions)
                {
                    case 0:
                        this.Status = RecommendationStatus.Success;
                        this.Summary = Resources.Localization.System_Repackaging_Recommendation_Option0;
                        break;
                    case 1:
                        this.Status = RecommendationStatus.Warning;
                        this.Summary = Resources.Localization.System_Repackaging_Recommendation_Option1;
                        break;
                    case 2:
                        this.Status = RecommendationStatus.Warning;
                        this.Summary = Resources.Localization.System_Repackaging_Recommendation_Option2;
                        break;
                    case 3:
                        this.Status = RecommendationStatus.Warning;
                        this.Summary = Resources.Localization.System_Repackaging_Recommendation_Option3;
                        break;
                    default:
                        this.Status = RecommendationStatus.Warning;
                        this.Summary = string.Format(Resources.Localization.System_Repackaging_Recommendation_OptionN, suggestedActions);
                        break;
                }
            }
            finally
            {
                this.IsLoading = false;
            }
        }

        public ObservableCollection<ServiceRecommendationViewModel> Items { get; private set; }

        protected override Geometry GetIcon()
        {
            return Geometry.Parse("M 21.5 2.5 L 21.5 3.90625 C 20.664063 4.054688 19.886719 4.371094 19.21875 4.84375 L 18.1875 3.875 L 16.8125 5.34375 L 17.8125 6.28125 C 17.363281 6.9375 17.050781 7.6875 16.90625 8.5 L 15.5 8.5 L 15.5 10.5 L 16.90625 10.5 C 17.050781 11.328125 17.378906 12.085938 17.84375 12.75 L 16.78125 13.78125 L 18.21875 15.21875 L 19.25 14.15625 C 19.914063 14.621094 20.671875 14.949219 21.5 15.09375 L 21.5 16.5 L 23.5 16.5 L 23.5 15.09375 C 24.3125 14.949219 25.0625 14.636719 25.71875 14.1875 L 26.65625 15.1875 L 28.125 13.8125 L 27.15625 12.78125 C 27.628906 12.113281 27.945313 11.335938 28.09375 10.5 L 29.5 10.5 L 29.5 8.5 L 28.09375 8.5 C 27.949219 7.671875 27.621094 6.914063 27.15625 6.25 L 28.09375 5.3125 L 26.6875 3.90625 L 25.75 4.84375 C 25.085938 4.378906 24.328125 4.050781 23.5 3.90625 L 23.5 2.5 Z M 22.5 5.8125 C 24.554688 5.8125 26.1875 7.445313 26.1875 9.5 C 26.1875 11.554688 24.554688 13.1875 22.5 13.1875 C 20.445313 13.1875 18.8125 11.554688 18.8125 9.5 C 18.8125 7.445313 20.445313 5.8125 22.5 5.8125 Z M 9.53125 11.71875 L 7.6875 12.46875 L 8.40625 14.28125 C 7.453125 14.851563 6.640625 15.648438 6.0625 16.59375 L 4.28125 15.875 L 3.53125 17.71875 L 5.3125 18.4375 C 5.179688 18.964844 5.09375 19.523438 5.09375 20.09375 C 5.09375 20.664063 5.179688 21.21875 5.3125 21.75 L 3.53125 22.46875 L 4.28125 24.3125 L 6.0625 23.59375 C 6.640625 24.554688 7.445313 25.359375 8.40625 25.9375 L 7.6875 27.71875 L 9.53125 28.46875 L 10.25 26.6875 C 10.78125 26.820313 11.332031 26.90625 11.90625 26.90625 C 12.476563 26.90625 13.035156 26.820313 13.5625 26.6875 L 14.28125 28.46875 L 16.125 27.71875 L 15.40625 25.9375 C 16.351563 25.359375 17.148438 24.546875 17.71875 23.59375 L 19.53125 24.3125 L 20.28125 22.46875 L 18.46875 21.75 C 18.601563 21.21875 18.6875 20.664063 18.6875 20.09375 C 18.6875 19.523438 18.601563 18.964844 18.46875 18.4375 L 20.28125 17.71875 L 19.53125 15.875 L 17.71875 16.59375 C 17.148438 15.652344 16.351563 14.851563 15.40625 14.28125 L 16.125 12.46875 L 14.28125 11.71875 L 13.5625 13.53125 C 13.03125 13.398438 12.476563 13.3125 11.90625 13.3125 C 11.335938 13.3125 10.78125 13.398438 10.25 13.53125 Z M 11.90625 15.3125 C 14.570313 15.3125 16.6875 17.429688 16.6875 20.09375 C 16.6875 22.757813 14.570313 24.90625 11.90625 24.90625 C 9.242188 24.90625 7.09375 22.757813 7.09375 20.09375 C 7.09375 17.429688 9.242188 15.3125 11.90625 15.3125 Z");
        }
    }
}
