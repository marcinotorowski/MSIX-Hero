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
using Otor.MsixHero.App.Mvvm.Progress;

namespace Otor.MsixHero.App.Modules.Main.Sidebar.Views
{
    /// <summary>
    /// Interaction logic for SidebarView.xaml
    /// </summary>
    public partial class SidebarView
    {
        private readonly IBusyManager busyManager;

        public SidebarView(IBusyManager busyManager)
        {
            this.busyManager = busyManager;
            this.InitializeComponent();

            this.busyManager.StatusChanged += this.OnBusyManagerStatusChanged;
        }

        private void OnBusyManagerStatusChanged(object sender, IBusyStatusChange e)
        {
            if (Application.Current == null)
            {
                this.IsEnabled = !e.IsBusy;
            }
            else if (Application.Current.CheckAccess())
            {
                this.IsEnabled = !e.IsBusy;
            }
            else
            {
                Application.Current.Dispatcher.Invoke(() => this.IsEnabled = !e.IsBusy);
            }
        }
    }
}
