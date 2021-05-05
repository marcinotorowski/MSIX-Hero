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
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using Otor.MsixHero.App.Controls.PackageExpert.ViewModels;
using Otor.MsixHero.App.Controls.PackageExpert.Views;
using Otor.MsixHero.App.Hero.Events;
using Otor.MsixHero.Appx.Diagnostic.RunningDetector;
using Otor.MsixHero.Appx.Packaging.Installation;
using Otor.MsixHero.Appx.Signing;
using Otor.MsixHero.Appx.Volumes;
using Otor.MsixHero.Infrastructure.Configuration;
using Otor.MsixHero.Infrastructure.Logging;
using Otor.MsixHero.Infrastructure.Processes;
using Otor.MsixHero.Infrastructure.Processes.SelfElevation;
using Otor.MsixHero.Infrastructure.Services;
using Prism.Common;
using Prism.Events;
using Prism.Regions;
using Prism.Services.Dialogs;

namespace Otor.MsixHero.App.Controls.PackageExpert
{
    public class PackageExpertControl : Control
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(PackageExpertControl));

        public static readonly DependencyProperty FilePathProperty = DependencyProperty.Register("FilePath", typeof(string), typeof(PackageExpertControl), new PropertyMetadata(null, OnFilePathChanged));
        
        private static readonly DependencyPropertyKey PackagePropertyKey = DependencyProperty.RegisterReadOnly("Package", typeof(PackageExpertViewModel), typeof(PackageExpertControl), new PropertyMetadata(null));
        public static readonly DependencyProperty PackageProperty = PackagePropertyKey.DependencyProperty;

        private static readonly DependencyPropertyKey ErrorMessagePropertyKey = DependencyProperty.RegisterReadOnly("ErrorMessage", typeof(string), typeof(PackageExpertControl), new PropertyMetadata(null));
        public static readonly DependencyProperty ErrorMessageProperty = ErrorMessagePropertyKey.DependencyProperty;

        private static readonly DependencyPropertyKey IsLoadingPropertyKey = DependencyProperty.RegisterReadOnly("IsLoading", typeof(string), typeof(PackageExpertControl), new PropertyMetadata(null));
        public static readonly DependencyProperty IsLoadingProperty = IsLoadingPropertyKey.DependencyProperty;

        public static readonly DependencyProperty ShowActionBarProperty = DependencyProperty.Register("ShowActionBar", typeof(bool), typeof(PackageExpertControl), new PropertyMetadata(true));

        static PackageExpertControl()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(PackageExpertControl), new FrameworkPropertyMetadata(typeof(PackageExpertControl)));
        }

        private readonly IInterProcessCommunicationManager ipcManager;
        private readonly ISelfElevationProxyProvider<IAppxPackageManager> packageManagerProvider;
        private readonly ISelfElevationProxyProvider<IAppxVolumeManager> volumeManagerProvider;
        private readonly IRunningAppsDetector runningDetector;
        private readonly IInteractionService interactionService;
        private readonly ISelfElevationProxyProvider<ISigningManager> signingManagerProvider;
        private readonly IDialogService dialogService;
        private readonly IConfigurationService configurationService;
        private readonly ObservableObject<object> context;
        private ActionBar actionBar;

        public PackageExpertControl(IEventAggregator eventAggregator,
            IInterProcessCommunicationManager ipcManager,
            ISelfElevationProxyProvider<IAppxPackageManager> packageManagerProvider,
            ISelfElevationProxyProvider<IAppxVolumeManager> volumeManagerProvider,
            ISelfElevationProxyProvider<ISigningManager> signingManagerProvider,
            IRunningAppsDetector runningDetector,
            IInteractionService interactionService,
            IDialogService dialogService,
            IConfigurationService configurationService
        )
        {
            this.context = RegionContext.GetObservableContext(this);
            this.context.PropertyChanged += this.OnPropertyChanged;
            this.ipcManager = ipcManager;
            this.packageManagerProvider = packageManagerProvider;
            this.volumeManagerProvider = volumeManagerProvider;
            this.runningDetector = runningDetector;
            this.interactionService = interactionService;
            this.signingManagerProvider = signingManagerProvider;
            this.dialogService = dialogService;
            this.configurationService = configurationService;

            eventAggregator.GetEvent<ToolsChangedEvent>().Subscribe(this.CreateTools, ThreadOption.UIThread);
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            this.actionBar = (ActionBar)this.GetTemplateChild("PART_ActionBar");
            this.CreateTools(null);
        }

        private async void CreateTools(IReadOnlyCollection<ToolListConfiguration> tools)
        {
            if (this.actionBar == null)
            {
                return;
            }
            
            if (tools == null)
            {
                var cfg = await this.configurationService.GetCurrentConfigurationAsync().ConfigureAwait(true);
                tools = cfg.Packages?.Tools;
            }

            this.Tools.Clear();

            if (tools == null || tools.Count == 0)
            {
                return;
            }

            foreach (var item in tools)
            {
                this.Tools.Add(new ToolItem(item));
            }

            this.actionBar.Tools = this.Tools;
        }
        
        private void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            this.FilePath = (string) this.context.Value;
        }

        public PackageExpertViewModel Package
        {
            get => (PackageExpertViewModel)GetValue(PackageProperty);
            private set => SetValue(PackagePropertyKey, value);
        }

        public string ErrorMessage
        {
            get => (string)GetValue(ErrorMessageProperty);
            private set => SetValue(ErrorMessagePropertyKey, value);
        }

        public bool ShowActionBar
        {
            get => (bool)GetValue(ShowActionBarProperty);
            set => SetValue(ShowActionBarProperty, value);
        }

        public bool IsLoading => (bool)GetValue(IsLoadingProperty);

        public string FilePath
        {
            get => (string)GetValue(FilePathProperty);
            set => SetValue(FilePathProperty, value);
        }

        public ObservableCollection<ToolItem> Tools { get; } = new ObservableCollection<ToolItem>();

        private static async void OnFilePathChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs eventArgs)
        {
            var sender = (PackageExpertControl)dependencyObject;
            var newFilePath = (string) eventArgs.NewValue;

            if (newFilePath == null)
            {
                sender.Package = null;
                return;
            }

            var newDataContext = new PackageExpertViewModel(
                newFilePath,
                sender.ipcManager,
                sender.packageManagerProvider,
                sender.volumeManagerProvider,
                sender.signingManagerProvider,
                sender.interactionService);

            try
            {
                await newDataContext.Load().ConfigureAwait(true);
                sender.Package = newDataContext;
                sender.ErrorMessage = null;
            }
            catch (Exception exception)
            {
                sender.Package = null;
                sender.ErrorMessage = "Could not load details. " + exception.Message;
                Logger.Warn($"Could not load details of package '{newFilePath}'.");
            }
        }
    }
}
