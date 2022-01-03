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

using System.Windows;
using Otor.MsixHero.App.Events;
using Otor.MsixHero.App.Hero;
using Otor.MsixHero.Appx.Volumes;
using Otor.MsixHero.Infrastructure.Processes.SelfElevation;
using Otor.MsixHero.Infrastructure.Services;
using Otor.MsixHero.Lib.Infrastructure.Progress;
using Prism.Modularity;
using Prism.Regions;
using Prism.Services.Dialogs;

namespace Otor.MsixHero.App.Modules.VolumeManagement.Views
{
    public partial class VolumeManagementView : INavigationAware
    {
        private readonly IMsixHeroApplication application;

        // ReSharper disable once NotAccessedField.Local
        private VolumeManagementHandler commandHandler;

        public VolumeManagementView(
            IMsixHeroApplication application,
            IInteractionService interactionService, 
            IConfigurationService configurationService, 
            ISelfElevationProxyProvider<IAppxVolumeManager> volumeManagerProvider,
            IBusyManager busyManager,
            IDialogService dialogService, 
            IModuleManager moduleManager)
        {
            this.application = application;
            this.InitializeComponent();
            this.commandHandler = new VolumeManagementHandler(
                this, 
                application, 
                interactionService, 
                configurationService,
                volumeManagerProvider, 
                busyManager, 
                dialogService,
                moduleManager);
        }

        public void OnNavigatedTo(NavigationContext navigationContext)
        {
            this.application.EventAggregator.GetEvent<TopSearchWidthChangeEvent>().Publish(new TopSearchWidthChangeEventPayLoad(this.Region.ActualWidth));
        }

        public bool IsNavigationTarget(NavigationContext navigationContext)
        {
            return true;
        }

        public void OnNavigatedFrom(NavigationContext navigationContext)
        {
        }

        private void RegionOnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            this.application.EventAggregator.GetEvent<TopSearchWidthChangeEvent>().Publish(new TopSearchWidthChangeEventPayLoad(this.Region.ActualWidth));
        }
    }
}
