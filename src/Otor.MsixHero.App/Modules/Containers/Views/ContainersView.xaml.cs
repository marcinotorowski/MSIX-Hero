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

using System.Windows;
using Otor.MsixHero.App.Hero;
using Otor.MsixHero.App.Modules.Main.Events;
using Prism.Navigation.Regions;

namespace Otor.MsixHero.App.Modules.Containers.Views
{
    /// <summary>
    /// Interaction logic for EventViewerView.
    /// </summary>
    public partial class ContainersView : INavigationAware
    {
        private readonly IMsixHeroApplication _application;

        public ContainersView(IMsixHeroApplication application)
        {
            this._application = application;
            this.InitializeComponent();
        }
        
        private void RegionOnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            this._application.EventAggregator.GetEvent<TopSearchWidthChangeEvent>().Publish(new TopSearchWidthChangeEventPayLoad(this.Region.ActualWidth));
        }

        public void OnNavigatedTo(NavigationContext navigationContext)
        {
            this._application.EventAggregator.GetEvent<TopSearchWidthChangeEvent>().Publish(new TopSearchWidthChangeEventPayLoad(this.Region.ActualWidth));
        }

        public bool IsNavigationTarget(NavigationContext navigationContext)
        {
            return true;
        }

        public void OnNavigatedFrom(NavigationContext navigationContext)
        {
        }
    }
}
