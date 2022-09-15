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

using System;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Dapplo.Log;
using Otor.MsixHero.App.Modules.Dialogs.Settings.ViewModel;
using Otor.MsixHero.App.Modules.Dialogs.Settings.ViewModel.Tools;
using Otor.MsixHero.Appx.Packaging;
using Otor.MsixHero.Appx.Packaging.Services;
using Otor.MsixHero.Infrastructure.Helpers;
using Otor.MsixHero.Infrastructure.Logging;
using Otor.MsixHero.Infrastructure.Services;

namespace Otor.MsixHero.App.Modules.Dialogs.Settings.View
{
    /// <summary>
    /// Interaction logic for SettingsView.xaml
    /// </summary>
    public partial class SettingsView
    {
        private static readonly LogSource Logger = new();
        private readonly IInteractionService _interactionService;
        private readonly IAppxPackageRunService _packageRunService;

        public SettingsView(
            IInteractionService interactionService,
            IAppxPackageRunService packageRunService)
        {
            this._interactionService = interactionService;
            this._packageRunService = packageRunService;
            this.InitializeComponent();
            this.DataContextChanged += this.OnDataContextChanged;

            if (this.DataContext is SettingsViewModel dataContext)
            {
                this.SetEntryPoint(dataContext.EntryPoint);
                dataContext.PropertyChanged += this.DataContextOnPropertyChanged;
            }
        }

        private void SetEntryPoint(string dataContextEntryPoint)
        {
            if (string.IsNullOrEmpty(dataContextEntryPoint))
            {
                return;
            }

            foreach (var tab in this.MainContent.Items.OfType<TabItem>())
            {
                if (string.Equals(tab.Name, dataContextEntryPoint, StringComparison.Ordinal))
                {
                    this.MainContent.SelectedItem = tab;
                    break;
                }
            }
        }

        private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue is SettingsViewModel dataContext)
            {
                this.SetEntryPoint(dataContext.EntryPoint);
                dataContext.PropertyChanged += this.DataContextOnPropertyChanged;
            }
        }

        private void DataContextOnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(SettingsViewModel.EntryPoint))
            {
                this.SetEntryPoint(((SettingsViewModel)sender).EntryPoint);
            }
        }

        private void CloseExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            // ReSharper disable once PossibleNullReferenceException
            Window.GetWindow(this).Close();
        }

        private void SaveExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            var closeWindow = e.Parameter != null && e.Parameter is bool parameter && parameter;

            ((SettingsViewModel)this.DataContext).Save().ContinueWith(t =>
                {
                    if (t.Exception == null && !t.IsCanceled && !t.IsFaulted && t.IsCompleted)
                    {
                        if (closeWindow && t.Result)
                        {
                            // ReSharper disable once PossibleNullReferenceException
                            Window.GetWindow(this).Close();
                        }
                    }
                    else if (t.IsFaulted && t.Exception != null)
                    {
                        var exception = t.Exception.GetBaseException();
                        var result = this._interactionService.ShowError(exception.Message, exception);
                        if (result == InteractionResult.Retry)
                        {
                            this.SaveExecuted(sender, e);
                        }
                    }
                },
                CancellationToken.None,
                TaskContinuationOptions.ExecuteSynchronously | TaskContinuationOptions.AttachedToParent,
                TaskScheduler.FromCurrentSynchronizationContext());
        }

        private void CanSave(object sender, CanExecuteRoutedEventArgs e)
        {
            var dataContext = ((SettingsViewModel)this.DataContext);
            e.CanExecute = dataContext.CanCloseDialog() && dataContext.CanSave();
            e.ContinueRouting = !e.CanExecute;
        }
        
        private void ToolsDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue is ToolsConfigurationViewModel dataContext)
            {
                dataContext.Items.CollectionChanged += this.ToolsCollectionChanged;
            }
        }

        private void ToolsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                this.ToolDisplayName.Focus();
                FocusManager.SetFocusedElement(this, this.ToolDisplayName);
                Keyboard.Focus(this.ToolDisplayName);
                this.ToolDisplayName.SelectAll();
            }
        }
        
        private void OpenLogsClicked(object sender, RoutedEventArgs e)
        {
            var logFile = LogManager.LogFile;
            if (logFile == null)
            {
                return;
            }

            var familyName = PackageIdentity.FromCurrentProcess()?.GetFamilyName();
            if (string.IsNullOrEmpty(familyName))
            {
                Logger.Info().WriteLine($"Opening log file {logFile} in notepad.exe...");

                ExceptionGuard.Guard(() =>
                {
                    var psi = new ProcessStartInfo
                    {
                        UseShellExecute = true,
                        FileName = "notepad.exe",
                        Arguments = "\"" + logFile + "\""
                    };

                    Process.Start(psi);
                });
            }
            else
            {
                var notepadPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System), "notepad.exe");
                Logger.Info().WriteLine($"Opening log file {logFile} in '{notepadPath}' (inside MSIX container)...");
                this._packageRunService.RunToolInContext(familyName, "MSIXHero", notepadPath, logFile);
            }
        }
    }
}
