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

using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using Otor.MsixHero.App.Modules.Dialogs.Volumes.ChangeVolume.ViewModel;
using Otor.MsixHero.App.Modules.Dialogs.Volumes.ChangeVolume.ViewModel.Items;

namespace Otor.MsixHero.App.Modules.Dialogs.Volumes.ChangeVolume.View
{
    /// <summary>
    /// Interaction logic for Change Volume View.
    /// </summary>
    public partial class ChangeVolumeView
    {
        public ChangeVolumeView()
        {
            InitializeComponent();
        }

        private void CreateNew(object sender, RoutedEventArgs e)
        {
            ((ChangeVolumeViewModel) this.DataContext).CreateNew();
        }

        private void Selector_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count == 0)
            {
                return;
            }

            var oldValue = e.RemovedItems?.OfType<VolumeCandidateViewModel>().FirstOrDefault();
            var newValue = e.AddedItems?.OfType<VolumeCandidateViewModel>().FirstOrDefault();

            if (newValue?.Name == null)
            {
                ((Selector) sender).SelectedValue = oldValue?.Name;
                ((ChangeVolumeViewModel)this.DataContext).CreateNew();
            }
        }
    }
}
