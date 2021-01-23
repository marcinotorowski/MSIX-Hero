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

using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using Otor.MsixHero.App.Modules.SystemStatus.ViewModel;
using Otor.MsixHero.App.Modules.SystemStatus.ViewModel.DeveloperMode;
using Otor.MsixHero.App.Modules.SystemStatus.ViewModel.Repackaging;
using Otor.MsixHero.App.Modules.SystemStatus.ViewModel.WindowsStoreUpdates;
using Otor.MsixHero.Appx.Diagnostic.Developer.Enums;
using Otor.MsixHero.Infrastructure.Logging;
using Otor.MsixHero.Infrastructure.Services;

namespace Otor.MsixHero.App.Modules.SystemStatus.View
{
    /// <summary>
    /// Interaction logic for System Status View.
    /// </summary>
    public partial class SystemStatusView
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(SystemStatusView));

        private readonly IInteractionService interactionService;

        public SystemStatusView(IInteractionService interactionService)
        {
            this.interactionService = interactionService;
            this.InitializeComponent();
        }

        private void OpenLink(object sender, RoutedEventArgs e)
        {
            string url;


            if (sender is Hyperlink link)
            {
                url = (string) link.Tag;
            }
            else if (sender is Button button)
            {
                url = (string) button.Tag;
            }
            else
            {
                return;
            }

            var psi = new ProcessStartInfo(url)
            {
                UseShellExecute = true
            };

            try
            {
                Process.Start(psi);
            }
            catch (Exception exception)
            {
                Logger.Error(exception);
                this.interactionService.ShowError($"Could not open the URL {url}", exception);
            }
        }

        private void WindowsSettingsExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            var sourceViewModel = (BaseRecommendationViewModel)e.Parameter;
            
            if (sourceViewModel is DeveloperAndSideloadingRecommendationViewModel)
            {
                var process = new ProcessStartInfo("ms-settings:developers") { UseShellExecute = true };
                Process.Start(process);
            }
            else if (sourceViewModel is RepackagingRecommendationViewModel)
            {
                var process = new ProcessStartInfo("services.msc") { UseShellExecute = true, Verb = "runas" };
                Process.Start(process);
            }
            else if (sourceViewModel is AutoDownloadRecommendationViewModel)
            {
                var process = new ProcessStartInfo("gpedit.msc") { UseShellExecute = true, Verb = "runas"};
                Process.Start(process);
            }
        }

        private void RefreshExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            ((SystemStatusViewModel)this.DataContext).Refresh();
        }

        private void StartExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            var serviceItem = (ServiceRecommendationViewModel)e.Parameter;
            serviceItem.Start();
        }

        private void StopExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            var serviceItem = (ServiceRecommendationViewModel)e.Parameter;
            serviceItem.Stop();
        }

        private void DisableUpdatesClick(object sender, RoutedEventArgs e)
        {
            var dataContext = (AutoDownloadRecommendationViewModel)((FrameworkElement)sender).DataContext;
            dataContext.AutoDownloadStatus = WindowsStoreAutoDownload.Never;
        }

        private void RestoreUpdatesClick(object sender, RoutedEventArgs e)
        {
            var hyperlink = (Hyperlink) sender;
            var dataContext = (AutoDownloadRecommendationViewModel)hyperlink.DataContext;
            dataContext.AutoDownloadStatus = WindowsStoreAutoDownload.Default;
        }
    }
}
