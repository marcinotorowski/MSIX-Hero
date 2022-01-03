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
using Prism.Events;
using Prism.Regions;

namespace Otor.MsixHero.App.Modules.PackageManagement.Views
{
    public partial class PackageManagementView : INavigationAware
    {
        private readonly IEventAggregator eventAggregator;

        public PackageManagementView(IEventAggregator eventAggregator)
        {
            this.eventAggregator = eventAggregator;
            this.InitializeComponent();
        }

        public void OnNavigatedTo(NavigationContext navigationContext)
        {
            this.eventAggregator.GetEvent<TopSearchWidthChangeEvent>().Publish(new TopSearchWidthChangeEventPayLoad(this.Region.ActualWidth));
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
            this.eventAggregator.GetEvent<TopSearchWidthChangeEvent>().Publish(new TopSearchWidthChangeEventPayLoad(this.Region.ActualWidth));
        }
    }
}
