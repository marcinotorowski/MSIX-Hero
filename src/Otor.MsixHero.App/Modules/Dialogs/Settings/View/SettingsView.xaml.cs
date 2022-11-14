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
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.Xaml.Behaviors;
using Otor.MsixHero.App.Helpers.Validation;
using Otor.MsixHero.App.Modules.Dialogs.Settings.Tabs;
using Otor.MsixHero.App.Modules.Dialogs.Settings.Tabs.AppAttach.ViewModel;
using Otor.MsixHero.App.Modules.Dialogs.Settings.Tabs.Commands.ViewModel;
using Otor.MsixHero.App.Modules.Dialogs.Settings.Tabs.Interface.ViewModel;
using Otor.MsixHero.App.Modules.Dialogs.Settings.Tabs.Other.ViewModel;
using Otor.MsixHero.App.Modules.Dialogs.Settings.Tabs.Signing.ViewModel;
using Otor.MsixHero.App.Modules.Dialogs.Settings.ViewModel;
using Otor.MsixHero.Infrastructure.Services;

namespace Otor.MsixHero.App.Modules.Dialogs.Settings.View
{
    /// <summary>
    /// Interaction logic for SettingsView.xaml
    /// </summary>
    public partial class SettingsView
    {
        private readonly IInteractionService _interactionService;

        public SettingsView(
            IInteractionService interactionService)
        {
            this._interactionService = interactionService;
            this.InitializeComponent();
            this.DataContextChanged += this.OnDataContextChanged;
            
            if (this.DataContext is SettingsViewModel dataContext)
            {
                this.SetEntryPoint(dataContext.EntryPoint);
                dataContext.PropertyChanged += this.DataContextOnPropertyChanged;
                dataContext.Context.ChangeableRegistered += this.ChangeableRegistered;
            }
        }

        private void ChangeableRegistered(object sender, ISettingsComponent e)
        {
            switch (e)
            {
                case AppAttachSettingsTabViewModel appAttach:
                {
                    var bc = Interaction.GetBehaviors(this.TabAppAttach);
                    bc.Add(new ValidationBehavior
                    {
                        ValidatedChangeable = appAttach
                    });

                    break;
                }

                case SigningSettingsTabViewModel signing:
                {
                    var bc = Interaction.GetBehaviors(this.TabSigning);
                    bc.Add(new ValidationBehavior
                    {
                        ValidatedChangeable = signing
                    });

                    break;
                }

                case CommandsSettingsTabViewModel commands:
                {
                    var bc = Interaction.GetBehaviors(this.TabCommands);
                    bc.Add(new ValidationBehavior
                    {
                        ValidatedChangeable = commands
                    });

                    break;
                }

                case OtherSettingsTabViewModel other:
                {
                    var bc = Interaction.GetBehaviors(this.TabCommands);
                    bc.Add(new ValidationBehavior
                    {
                        ValidatedChangeable = other
                    });

                    break;
                }

                case InterfaceSettingsTabViewModel inter:
                {
                    var bc = Interaction.GetBehaviors(this.TabInterface);
                    bc.Add(new ValidationBehavior
                    {
                        ValidatedChangeable = inter
                    });

                    break;
                }
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

                dataContext.PropertyChanged -= this.DataContextOnPropertyChanged;
                dataContext.Context.ChangeableRegistered -= this.ChangeableRegistered;

                dataContext.PropertyChanged += this.DataContextOnPropertyChanged;
                dataContext.Context.ChangeableRegistered += this.ChangeableRegistered;
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
    }
}
