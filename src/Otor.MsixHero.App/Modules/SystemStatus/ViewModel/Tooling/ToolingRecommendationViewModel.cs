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
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media;
using Otor.MsixHero.Appx.Diagnostic.Recommendations.Entities;
using Otor.MsixHero.Appx.Diagnostic.Recommendations.ThirdParty;

namespace Otor.MsixHero.App.Modules.SystemStatus.ViewModel.Tooling
{
    public class ToolingRecommendationViewModel : BaseRecommendationViewModel
    {
        protected readonly IThirdPartyAppProvider ThirdPartyDetector;

        public ToolingRecommendationViewModel(IThirdPartyAppProvider thirdPartyDetector)
        {
            this.ThirdPartyDetector = thirdPartyDetector;
            this.Items = new ObservableCollection<DiscoveredAppViewModel>();
        }

        protected override void OnCultureChanged()
        {
            this.SetStatusAndSummary();
            base.OnCultureChanged();
        }

        protected override Geometry GetIcon()
        {
            return Geometry.Parse("M 4 4 L 4 28.03125 L 28.03125 28.03125 L 28.03125 22 L 23 22 L 23 4 Z M 6 6 L 15 6 L 15 22 L 6 22 Z M 17 6 L 21 6 L 21 22 L 17 22 Z M 8 8 L 8 10 L 13 10 L 13 8 Z M 8 12 L 8 14 L 13 14 L 13 12 Z M 17.875 19 L 17.875 21 L 20 21 L 20 19 Z M 6 24 L 26.03125 24 L 26.03125 26.03125 L 6 26.03125 Z");
        }

        public override async Task Refresh(CancellationToken cancellationToken = default)
        {
            this.IsLoading = true;

            try
            {
                await Task.Delay(1000, cancellationToken).ConfigureAwait(false);
                var thirdParty = await Task.Run(() => this.ThirdPartyDetector.ProvideApps().ToList(), cancellationToken).ConfigureAwait(false);

                this.Items = new ObservableCollection<DiscoveredAppViewModel>();
                foreach (var app in thirdParty)
                {
                    if (app is IThirdPartyDetectedApp)
                    {
                        this.Items.Add(new DiscoveredAppViewModel(app, DiscoveredAppViewModelStatus.Installed));
                    }
                    else if (app is IStoreApp)
                    {
                        this.Items.Add(new DiscoveredAppViewModel(app, DiscoveredAppViewModelStatus.Available));
                    }
                    else
                    {
                        this.Items.Add(new DiscoveredAppViewModel(app));
                    }
                }

                this.OnPropertyChanged(nameof(this.Items));
                this.SetStatusAndSummary();
            }
            finally
            {
                this.IsLoading = false;
            }
        }

        private void SetStatusAndSummary()
        {
            switch (this.Items.Count(item => item.Status == DiscoveredAppViewModelStatus.Installed))
            {
                case 0:
                    this.Summary = Resources.Localization.System_Apps_Recommendation_Option0;
                    this.Status = RecommendationStatus.Warning;
                    break;
                case 1:
                    this.Status = RecommendationStatus.Success;
                    this.Summary = Resources.Localization.System_Apps_Recommendation_Option1;
                    break;
                case 2:
                    this.Status = RecommendationStatus.Success;
                    this.Summary = Resources.Localization.System_Apps_Recommendation_Option2;
                    break;
                case 3:
                    this.Status = RecommendationStatus.Success;
                    this.Summary = Resources.Localization.System_Apps_Recommendation_Option3;
                    break;
                default:
                    this.Status = RecommendationStatus.Success;
                    this.Summary = string.Format(Resources.Localization.System_Apps_Recommendation_OptionN, this.Items.Count);
                    break;
            }
        }

        public ObservableCollection<DiscoveredAppViewModel> Items { get; private set; }

        public override string Title => Resources.Localization.System_Apps_Recommendation;
    }
}
