﻿// MSIX Hero
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

using System.Linq;
using System.Windows.Controls;
using System.Windows.Input;
using Otor.MsixHero.App.Hero;
using Otor.MsixHero.App.Hero.Commands.Volumes;
using Otor.MsixHero.App.Hero.Events.Base;
using Otor.MsixHero.App.Modules.VolumeManagement.ViewModels;
using Prism.Events;

namespace Otor.MsixHero.App.Modules.VolumeManagement.Views
{
    /// <summary>
    /// Interaction logic for VolumesListView.
    /// </summary>
    public partial class VolumesListView
    {
        private readonly IMsixHeroApplication application;

        public VolumesListView(IMsixHeroApplication application)
        {
            InitializeComponent();
            this.application = application;

            application.EventAggregator.GetEvent<UiFailedEvent<GetVolumesCommand>>().Subscribe(this.OnGetVolumesFailed, ThreadOption.UIThread);
            application.EventAggregator.GetEvent<UiExecutingEvent<GetVolumesCommand>>().Subscribe(this.OnGetVolumesExecuting);
            application.EventAggregator.GetEvent<UiCancelledEvent<GetVolumesCommand>>().Subscribe(this.OnGetVolumesCancelled, ThreadOption.UIThread);
            application.EventAggregator.GetEvent<UiExecutedEvent<GetVolumesCommand>>().Subscribe(this.OnGetVolumesExecuted, ThreadOption.UIThread);
            this.InitializeComponent();
            this.ListBox.PreviewKeyDown += ListBoxOnKeyDown;
            this.ListBox.PreviewKeyUp += ListBoxOnKeyUp;
        }

        private void ListBoxOnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Down || e.Key == Key.Up || e.Key == Key.PageDown || e.Key == Key.PageUp)
            {
                this.ListBox.SelectionChanged -= this.OnSelectionChanged;
            }
        }

        private void ListBoxOnKeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Down || e.Key == Key.Up || e.Key == Key.PageDown || e.Key == Key.PageUp)
            {
                this.ListBox.SelectionChanged += this.OnSelectionChanged;

                this.application.CommandExecutor.Invoke(this, new SelectVolumesCommand(this.ListBox.SelectedItems.OfType<VolumeViewModel>().Select(p => p.PackageStorePath)));
            }
        }

        private void OnGetVolumesExecuting(UiExecutingPayload<GetVolumesCommand> obj)
        {
            this.ListBox.SelectionChanged -= this.OnSelectionChanged;
        }

        private void OnGetVolumesFailed(UiFailedPayload<GetVolumesCommand> obj)
        {
            this.ListBox.SelectionChanged += this.OnSelectionChanged;
        }

        private void OnGetVolumesCancelled(UiCancelledPayload<GetVolumesCommand> obj)
        {
            this.ListBox.SelectionChanged += this.OnSelectionChanged;
        }

        private void OnGetVolumesExecuted(UiExecutedPayload<GetVolumesCommand> obj)
        {
            this.ListBox.SelectedItems.Clear();

            foreach (var item in this.ListBox.Items.OfType<VolumeViewModel>())
            {
                if (!this.application.ApplicationState.Volumes.SelectedVolumes.Contains(item.Model))
                {
                    continue;
                }

                this.ListBox.SelectedItems.Add(item);
            }

            this.ListBox.SelectionChanged += this.OnSelectionChanged;
        }
        
        private void OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            this.application.CommandExecutor.Invoke(this, new SelectVolumesCommand(this.ListBox.SelectedItems.OfType<VolumeViewModel>().Select(p => p.PackageStorePath)));
        }
    }
}
