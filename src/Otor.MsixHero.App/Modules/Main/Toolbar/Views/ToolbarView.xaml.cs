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

using Otor.MsixHero.App.Events;
using Prism.Events;
using Prism.Regions;

namespace Otor.MsixHero.App.Modules.Main.Toolbar.Views
{
    /// <summary>
    /// Interaction logic for ToolbarView.
    /// </summary>
    public partial class ToolbarView
    {
        private readonly IEventAggregator eventAggregator;
        private readonly IRegionManager regionManager;

        public ToolbarView(IEventAggregator eventAggregator, IRegionManager regionManager)
        {
            this.eventAggregator = eventAggregator;
            this.regionManager = regionManager;
            InitializeComponent();
            this.eventAggregator.GetEvent<TopSearchWidthChangeEvent>().Subscribe(this.OnTopSearchWidthChangeEvent, ThreadOption.UIThread);
        }

        private void OnTopSearchWidthChangeEvent(TopSearchWidthChangeEventPayLoad obj)
        {
            this.Region.Width = obj.PanelWidth;
        }
    }
}
