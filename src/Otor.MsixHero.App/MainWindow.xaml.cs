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
using System.Windows.Input;
using System.Windows.Shell;
using Otor.MsixHero.App.Modules;
using Prism.Modularity;
using Prism.Services.Dialogs;

namespace Otor.MsixHero.App
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        private readonly IModuleManager moduleManager;
        private readonly IDialogService dialogService;

        public MainWindow(IModuleManager moduleManager, IDialogService dialogService)
        {
            this.moduleManager = moduleManager;
            this.dialogService = dialogService;
            InitializeComponent();
            this.Loaded += OnLoaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            WindowChrome.GetWindowChrome(this).CaptionHeight = 55;
        }

        private void HelpExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            this.moduleManager.LoadModule(ModuleNames.Dialogs.Help);
            this.dialogService.ShowDialog(NavigationPaths.DialogPaths.About);
        }
    }
}
