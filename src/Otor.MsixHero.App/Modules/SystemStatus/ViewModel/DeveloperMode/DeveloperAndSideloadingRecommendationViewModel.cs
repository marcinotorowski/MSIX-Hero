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
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media;
using Otor.MsixHero.Appx.Diagnostic.Developer;
using Otor.MsixHero.Appx.Diagnostic.Developer.Enums;
using Otor.MsixHero.Infrastructure.Localization;

namespace Otor.MsixHero.App.Modules.SystemStatus.ViewModel.DeveloperMode
{
    public class DeveloperAndSideloadingRecommendationViewModel : BaseRecommendationViewModel
    {
        protected readonly ISideloadingConfigurator SideloadingConfigurator;
        private SideloadingStatus sideloadingStatus;

        public DeveloperAndSideloadingRecommendationViewModel(ISideloadingConfigurator sideloadingConfigurator)
        {
            this.IsLegacyFlavor = new Lazy<bool>(() => sideloadingConfigurator.Flavor == SideloadingFlavor.Windows10Below2004);
            this.SideloadingConfigurator = sideloadingConfigurator;
            this.SetStatusAndSummary();
        }

        protected override void OnCultureChanged()
        {
            this.SetStatusAndSummary();
            base.OnCultureChanged();
        }

        public Lazy<bool> IsLegacyFlavor { get; }

        public override string Title => Resources.Localization.System_Sideloading_Recommendation_Title;

        public SideloadingStatus SideloadingStatus
        {
            get => this.sideloadingStatus;
            set
            {
                if (this.sideloadingStatus == value)
                {
                    return;
                }

                if (!this.SideloadingConfigurator.Set(value))
                {
                    return;
                }

                this.sideloadingStatus = value;
                this.OnPropertyChanged();
                this.SetStatusAndSummary();
            }
        }
        
        public override Task Refresh(CancellationToken cancellationToken = default)
        {
            this.SetStatusAndSummary();
            this.OnPropertyChanged(nameof(this.SideloadingStatus));
            return Task.FromResult(true);
        }

        protected override Geometry GetIcon()
        {
            return Geometry.Parse("M 10.71875 3.28125 L 9.28125 4.71875 L 11.21875 6.65625 C 9.757813 7.773438 8.609375 9.410156 7.875 11.3125 L 5.4375 10.09375 L 4.5625 11.90625 L 7.3125 13.28125 C 7.128906 14.15625 7 15.0625 7 16 C 7 16.339844 7.007813 16.667969 7.03125 17 L 4 17 L 4 19 L 7.375 19 C 7.617188 20.042969 7.9375 21.039063 8.40625 21.9375 L 5.40625 24.1875 L 6.59375 25.8125 L 9.53125 23.625 C 11.148438 25.679688 13.417969 27 16 27 C 18.582031 27 20.851563 25.679688 22.46875 23.625 L 25.40625 25.8125 L 26.59375 24.1875 L 23.59375 21.9375 C 24.0625 21.039063 24.382813 20.042969 24.625 19 L 28 19 L 28 17 L 24.96875 17 C 24.992188 16.667969 25 16.339844 25 16 C 25 15.0625 24.871094 14.15625 24.6875 13.28125 L 27.4375 11.90625 L 26.5625 10.09375 L 24.125 11.3125 C 23.390625 9.410156 22.242188 7.773438 20.78125 6.65625 L 22.71875 4.71875 L 21.28125 3.28125 L 18.96875 5.59375 C 18.046875 5.203125 17.046875 5 16 5 C 14.953125 5 13.953125 5.203125 13.03125 5.59375 Z M 16 7 C 17.976563 7 19.828125 8.09375 21.125 9.875 C 19.992188 10.386719 18.199219 11 16 11 C 13.800781 11 12.007813 10.386719 10.875 9.875 C 12.171875 8.09375 14.023438 7 16 7 Z M 9.90625 11.59375 C 11.058594 12.136719 12.828125 12.773438 15 12.9375 L 15 24.90625 C 11.699219 24.28125 9 20.628906 9 16 C 9 14.382813 9.335938 12.886719 9.90625 11.59375 Z M 22.09375 11.59375 C 22.664063 12.886719 23 14.382813 23 16 C 23 20.628906 20.300781 24.28125 17 24.90625 L 17 12.9375 C 19.171875 12.773438 20.941406 12.136719 22.09375 11.59375 Z");
        }

        private void SetStatusAndSummary()
        {
            this.sideloadingStatus = this.SideloadingConfigurator.Get();

            switch (this.SideloadingStatus)
            {
                case SideloadingStatus.NotAllowed:
                    this.Status = RecommendationStatus.Warning;
                    this.Summary = Resources.Localization.System_Sideloading_Recommendation_Option1;
                    break;
                case SideloadingStatus.Sideloading:
                    this.Status = RecommendationStatus.Success;
                    this.Summary = Resources.Localization.System_Sideloading_Recommendation_Option2;
                    break;
                case SideloadingStatus.DeveloperMode:
                    this.Status = RecommendationStatus.Success;
                    this.Summary = Resources.Localization.System_Sideloading_Recommendation_Option3;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}