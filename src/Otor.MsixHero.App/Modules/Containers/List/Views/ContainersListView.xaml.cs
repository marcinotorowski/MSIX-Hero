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

using System.Linq;
using System.Windows.Controls;
using System.Windows.Input;
using Otor.MsixHero.App.Hero;
using Otor.MsixHero.App.Hero.Commands.Containers;
using Otor.MsixHero.App.Hero.Events.Base;
using Otor.MsixHero.App.Modules.Containers.List.ViewModels;
using Prism.Events;

namespace Otor.MsixHero.App.Modules.Containers.List.Views
{
    /// <summary>
    /// Interaction logic for EventViewerListView.
    /// </summary>
    public partial class ContainersListView
    {
        private readonly IMsixHeroApplication _application;
        
        public ContainersListView(IMsixHeroApplication application)
        {
            this._application = application;
            this.InitializeComponent();

            this._application.EventAggregator.GetEvent<UiFailedEvent<GetSharedPackageContainersCommand>>().Subscribe(this.OnGetFailed, ThreadOption.UIThread);
            this._application.EventAggregator.GetEvent<UiExecutingEvent<GetSharedPackageContainersCommand>>().Subscribe(this.OnGetExecuting);
            this._application.EventAggregator.GetEvent<UiCancelledEvent<GetSharedPackageContainersCommand>>().Subscribe(this.OnGetCancelled, ThreadOption.UIThread);
            this._application.EventAggregator.GetEvent<UiExecutedEvent<GetSharedPackageContainersCommand>>().Subscribe(this.OnGetExecuted, ThreadOption.UIThread);

            this.InitializeComponent();
            this.ListBox.PreviewKeyDown += ListBoxOnKeyDown;
            this.ListBox.PreviewKeyUp += ListBoxOnKeyUp;
        }
        private void OnGetExecuting(UiExecutingPayload<GetSharedPackageContainersCommand> obj)
        {
            this.ListBox.SelectionChanged -= this.OnSelectionChanged;
        }

        private void OnGetFailed(UiFailedPayload<GetSharedPackageContainersCommand> obj)
        {
            this.ListBox.SelectionChanged += this.OnSelectionChanged;
        }

        private void OnGetCancelled(UiCancelledPayload<GetSharedPackageContainersCommand> obj)
        {
            this.ListBox.SelectionChanged += this.OnSelectionChanged;
        }

        private void OnGetExecuted(UiExecutedPayload<GetSharedPackageContainersCommand> obj)
        {
            this.ListBox.SelectedItem = this.ListBox.Items.OfType<SharedPackageContainerViewModel>().FirstOrDefault(item => this._application.ApplicationState.Containers.SelectedContainer == item.Model);

            this.ListBox.SelectionChanged += this.OnSelectionChanged;
        }

        private void OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            this._application.CommandExecutor.Invoke(this, new SelectSharedPackageContainerCommand((this.ListBox.SelectedItem as SharedPackageContainerViewModel)?.Model));
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

                this._application.CommandExecutor.Invoke(this, new SelectSharedPackageContainerCommand((this.ListBox.SelectedItem as SharedPackageContainerViewModel)?.Model));
            }
        }
    }
}
