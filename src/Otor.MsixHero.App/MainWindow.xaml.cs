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

using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Shell;
using Otor.MsixHero.App.Helpers;
using Otor.MsixHero.App.Modules;
using Otor.MsixHero.Appx.Packaging;
using Otor.MsixHero.Infrastructure.Services;
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
        private readonly DialogOpener dialogOpener;

        public MainWindow(IModuleManager moduleManager, IDialogService dialogService, IInteractionService interactionService)
        {
            this.moduleManager = moduleManager;
            this.dialogService = dialogService;
            InitializeComponent();
            this.Loaded += OnLoaded;
            this.dialogOpener = new DialogOpener(moduleManager, dialogService, interactionService);
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

        private void OpenExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            // ReSharper disable once StringLiteralTypo
            var filterBuilder = new DialogFilterBuilder("*" + FileConstants.MsixExtension, "*" + FileConstants.AppxExtension, FileConstants.AppxManifestFile, "*" + FileConstants.WingetExtension, "*" + FileConstants.AppInstallerExtension);
            this.dialogOpener.ShowFileDialog(filterBuilder.BuildFilter());
        }

        private void OnFileDropped(object sender, DragEventArgs e)
        {
            DropFileObject.SetIsDragging((DependencyObject)sender, false);
            var hasData = e.Data.GetDataPresent("FileDrop");
            if (!hasData)
            {
                return;
            }

            var data = e.Data.GetData("FileDrop") as string[];
            if (data == null || !data.Any())
            {
                return;
            }

            var dropped = new FileInfo(data.First());
            this.dialogOpener.OpenFile(dropped);
        }

        private void OnDragEnter(object sender, DragEventArgs e)
        {
            DropFileObject.SetIsDragging((DependencyObject)sender, true);
        }

        private void OnDragLeave(object sender, DragEventArgs e)
        {
            DropFileObject.SetIsDragging((DependencyObject)sender, false);
        }
    }
}
