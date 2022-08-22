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
using Otor.MsixHero.App.Mvvm.Changeable;
using Otor.MsixHero.App.Mvvm.Progress;
using Otor.MsixHero.Appx.Packaging.Installation;
using Otor.MsixHero.Elevation;
using Otor.MsixHero.Infrastructure.Services;
using Prism.Services.Dialogs;

namespace Otor.MsixHero.App.Modules.Dialogs.PackageExpert.ViewModels
{
    public class PackageExpertDialogViewModel : NotifyPropertyChanged, IDialogAware
    {
        public PackageExpertDialogViewModel(
            IAppxPackageInstaller packageQueryInstaller,
            IAppxPackageQuery query,
            IMsixHeroApplication application,
            IInteractionService interactionService,
            IUacElevation uacElevation,
            PrismServices prismServices,
            IBusyManager busyManager,
            IConfigurationService configurationService)
        {
            this.CommandHandler = new PackageExpertCommandHandler(
                application,
                query,
                packageQueryInstaller,
                interactionService,
                configurationService,
                prismServices,
                uacElevation,
                busyManager);
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

        public ChangeableProperty<bool> IsInstalled { get; } = new ChangeableProperty<bool>();
        
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

            this.FilePath = packagePath;
            this.OnPropertyChanged(nameof(FilePath));

            this.Title = Path.GetFileName(this.FilePath);
            this.OnPropertyChanged(nameof(Title));

            this.CommandHandler.FilePath = packagePath;
        }

        public string Title { get; private set; } = "MSIX Hero";
        
        public event Action<IDialogResult> RequestClose;
        
        public string FilePath { get; private set; }
    }
}
