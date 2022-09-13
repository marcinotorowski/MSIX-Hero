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
using System.IO;
using System.Linq;
using Otor.MsixHero.App.Hero;
using Otor.MsixHero.App.Mvvm;
using Otor.MsixHero.App.Mvvm.Progress;
using Otor.MsixHero.Appx.Packaging.Services;
using Otor.MsixHero.Elevation;
using Otor.MsixHero.Infrastructure.Services;
using Prism.Services.Dialogs;

namespace Otor.MsixHero.App.Modules.Dialogs.PackageExpert.ViewModels
{
    public class PackageExpertDialogViewModel : NotifyPropertyChanged, IDialogAware
    {
        public PackageExpertDialogViewModel(
            IAppxPackageInstallationService packageQueryInstallationService,
            IAppxPackageQueryService queryService,
            IMsixHeroApplication application,
            IInteractionService interactionService,
            IUacElevation uacElevation,
            PrismServices prismServices,
            IBusyManager busyManager,
            IConfigurationService configurationService)
        {
            this.CommandHandler = new PackageExpertCommandHandler(
                application,
                queryService,
                packageQueryInstallationService,
                interactionService,
                configurationService,
                prismServices,
                uacElevation,
                busyManager);

            busyManager.StatusChanged += this.BusyManagerOnStatusChanged;
        }

        public PackageExpertCommandHandler CommandHandler { get; }

        public bool CanCloseDialog()
        {
            return true;
        }

        public void OnDialogClosed()
        {
            this.RequestClose?.Invoke(new DialogResult());
        }
        
        public void OnDialogOpened(IDialogParameters parameters)
        {
            var firstParam = parameters.Keys.FirstOrDefault();
            if (firstParam == null)
            {
                return;
            }

            if (!parameters.TryGetValue(firstParam, out string packagePath))
            {
                return;
            }

            this.Title = $"MSIX Hero - {Path.GetFileName(packagePath)}";
            this.OnPropertyChanged(nameof(Title));

            this.CommandHandler.FilePath = packagePath;
        }

        public string Title { get; private set; } = "MSIX Hero";
        
        public event Action<IDialogResult> RequestClose;

        public ProgressProperty Progress { get; } = new ProgressProperty();

        private void BusyManagerOnStatusChanged(object sender, IBusyStatusChange e)
        {
            if (e.Type != OperationType.PackageLoading && e.Type != OperationType.Other)
            {
                return;
            }

            this.Progress.IsLoading = e.IsBusy;
            this.Progress.Message = e.Message;
            this.Progress.Progress = e.Progress;
        }
    }
}
