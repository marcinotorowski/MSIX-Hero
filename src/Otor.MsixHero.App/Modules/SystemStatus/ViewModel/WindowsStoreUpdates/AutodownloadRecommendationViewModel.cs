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
using Otor.MsixHero.Appx.Diagnostic.Developer.Enums;
using Otor.MsixHero.Appx.Diagnostic.Store;

namespace Otor.MsixHero.App.Modules.SystemStatus.ViewModel.WindowsStoreUpdates
{
    public class AutoDownloadRecommendationViewModel : BaseRecommendationViewModel
    {
        protected readonly IWindowsStoreAutoDownloadConfigurator WindowsStoreAutoDownloadConfigurator;
        private WindowsStoreAutoDownload autoDownloadStatus;

        public AutoDownloadRecommendationViewModel(IWindowsStoreAutoDownloadConfigurator windowsStoreAutoDownloadConfigurator)
        {
            this.WindowsStoreAutoDownloadConfigurator = windowsStoreAutoDownloadConfigurator;
            this.SetStatusAndSummary();
        }

        protected override void OnCultureChanged()
        {
            this.SetStatusAndSummary();
            base.OnCultureChanged();
        }

        protected override Geometry GetIcon()
        {
            return Geometry.Parse("M 16 4 C 12.426807 4 9.1889768 5.5940841 7 8.1113281 L 7 5 L 5 5 L 5 12 L 12 12 L 12 10 L 8.0546875 10 C 9.8580114 7.5851367 12.758494 6 16 6 C 20.288652 6 23.809592 8.6271915 25.267578 12.363281 L 27.130859 11.636719 C 25.388846 7.1728084 21.111348 4 16 4 z M 6.7324219 19.636719 L 4.8691406 20.363281 C 6.6111543 24.827191 10.888652 28 16 28 C 19.599779 28 22.811088 26.3919 25 23.916016 L 25 27 L 27 27 L 27 20 L 20 20 L 20 22 L 23.96875 22 C 22.155848 24.387604 19.278082 26 16 26 C 11.711348 26 8.1904082 23.372809 6.7324219 19.636719 z");
        }

        private void SetStatusAndSummary()
        {
            this.autoDownloadStatus = this.WindowsStoreAutoDownloadConfigurator.Get();

            switch (this.AutoDownloadStatus)
            {
                case WindowsStoreAutoDownload.Default:
                    this.Status = RecommendationStatus.Success;
                    this.Summary = Resources.Localization.System_AutoDownload_Recommendation_Option0;
                    break;
                case WindowsStoreAutoDownload.Always:
                    this.Status = RecommendationStatus.Success;
                    this.Summary = Resources.Localization.System_AutoDownload_Recommendation_Option1;
                    break;
                case WindowsStoreAutoDownload.Never:
                    this.Status = RecommendationStatus.Success;
                    this.Summary = Resources.Localization.System_AutoDownload_Recommendation_Option2;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public WindowsStoreAutoDownload AutoDownloadStatus
        {
            get => this.autoDownloadStatus;
            set
            {
                if (this.autoDownloadStatus == value)
                {
                    return;
                }

                if (!this.WindowsStoreAutoDownloadConfigurator.Set(value))
                {
                    return;
                }

                this.autoDownloadStatus = value;
                this.OnPropertyChanged();
                this.SetStatusAndSummary();
            }
        }
        
        public override string Title => Resources.Localization.System_AutoDownload_Recommendation;

        public override Task Refresh(CancellationToken cancellationToken = default)
        {
            this.SetStatusAndSummary();
            this.OnPropertyChanged(nameof(this.AutoDownloadStatus));
            return Task.FromResult(true);
        }
    }
}