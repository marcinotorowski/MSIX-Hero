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
using System.Windows;
using Prism.Modularity;
using Prism.Regions;
using Prism.Services.Dialogs;

namespace Otor.MsixHero.App.Modules.WhatsNew.Views.Tabs
{
    public partial class WhatsNew1
    {
        private readonly IDialogService dialogService;
        private readonly IRegionNavigationService navigationService;
        private readonly IModuleManager moduleManager;

        public WhatsNew1(IDialogService dialogService, IRegionNavigationService navigationService, IModuleManager moduleManager)
        {
            this.dialogService = dialogService;
            this.navigationService = navigationService;
            this.moduleManager = moduleManager;
            InitializeComponent();
        }

        private void GoToDashboard(object sender, RoutedEventArgs e)
        {
            this.moduleManager.LoadModule(ModuleNames.Dashboard);
            this.navigationService.RequestNavigate(new Uri(NavigationPaths.Dashboard, UriKind.RelativeOrAbsolute));
        }

        private void CreateAppInstaller(object sender, RoutedEventArgs e)
        {
            this.moduleManager.LoadModule(ModuleNames.Dialogs.AppInstaller);
            this.dialogService.ShowDialog(NavigationPaths.DialogPaths.AppInstallerEditor, new DialogParameters(), result => { });
        }

        private void GoToSettings(object sender, RoutedEventArgs e)
        {
            this.moduleManager.LoadModule(ModuleNames.Dialogs.Settings);
            this.dialogService.ShowDialog(NavigationPaths.DialogPaths.Settings, new DialogParameters(), result => { });
        }
    }
}
