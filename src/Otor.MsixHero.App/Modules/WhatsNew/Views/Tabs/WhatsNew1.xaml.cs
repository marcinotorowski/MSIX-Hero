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
using Otor.MsixHero.App.Hero.Commands;
using Otor.MsixHero.App.Hero.Executor;
using Otor.MsixHero.App.Hero.State;
using Prism.Modularity;
using Prism.Regions;
using Prism.Services.Dialogs;

namespace Otor.MsixHero.App.Modules.WhatsNew.Views.Tabs
{
    public partial class WhatsNew1
    {
        private readonly IMsixHeroCommandExecutor commandExecutor;
        private readonly IDialogService dialogService;
        private readonly IRegionNavigationService navigationService;
        private readonly IModuleManager moduleManager;

        public WhatsNew1(
            IMsixHeroCommandExecutor commandExecutor,
            IDialogService dialogService, 
            IRegionNavigationService navigationService, 
            IModuleManager moduleManager)
        {
            this.commandExecutor = commandExecutor;
            this.dialogService = dialogService;
            this.navigationService = navigationService;
            this.moduleManager = moduleManager;
            InitializeComponent();
        }

        private void GoToPackages(object sender, RoutedEventArgs e)
        {
            this.commandExecutor.Invoke(this, new SetCurrentModeCommand(ApplicationMode.Packages));
        }

        private void CreateWingetManifest(object sender, RoutedEventArgs e)
        {
            this.moduleManager.LoadModule(ModuleNames.Dialogs.Winget);
            this.dialogService.ShowDialog(NavigationPaths.DialogPaths.WingetYamlEditor, new DialogParameters(), result => { });
        }

        private void ConvertToAppAttach(object sender, RoutedEventArgs e)
        {
            this.moduleManager.LoadModule(ModuleNames.Dialogs.AppAttach);
            this.dialogService.ShowDialog(NavigationPaths.DialogPaths.AppAttachEditor, new DialogParameters(), result => { });
        }
    }
}
