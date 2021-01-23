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

using System.Windows.Controls;
using System.Windows.Input;
using Otor.MsixHero.App.Modules.EventViewer.Search.ViewModels;
using Otor.MsixHero.App.Modules.PackageManagement.Search.ViewModels;

namespace Otor.MsixHero.App.Modules.EventViewer.Search.Views
{
    /// <summary>
    /// Interaction logic for EventViewerFilterSortView
    /// </summary>
    public partial class EventViewerFilterSortView
    {
        public EventViewerFilterSortView()
        {
            InitializeComponent();
        }

        private void UIElement_OnPreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            var senderRadio = (RadioButton)sender;
            if (senderRadio.IsChecked != true)
            {
                return;
            }

            if (e.Source as RadioButton != senderRadio)
            {
                return;
            }

            var dataContext = (EventViewerFilterSortViewModel) senderRadio.DataContext;
            dataContext.IsDescending = !dataContext.IsDescending;
        }
    }
}
