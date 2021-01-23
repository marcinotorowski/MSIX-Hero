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
using System.IO;
using System.Linq;
using Otor.MsixHero.App.Mvvm;
using Otor.MsixHero.App.Mvvm.Changeable;
using Otor.MsixHero.Appx.Packaging.Installation;
using Otor.MsixHero.Infrastructure.Processes.SelfElevation;
using Otor.MsixHero.Infrastructure.Processes.SelfElevation.Enums;
using Prism.Services.Dialogs;

namespace Otor.MsixHero.App.Dialogs.ViewModels
{
    public class PackageExpertDialogViewModel : NotifyPropertyChanged, IDialogAware
    {
        private readonly ISelfElevationProxyProvider<IAppxPackageManager> packageManagerProvider;

        public PackageExpertDialogViewModel(ISelfElevationProxyProvider<IAppxPackageManager> packageManagerProvider)
        {
            this.packageManagerProvider = packageManagerProvider;
        }

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

            this.CheckIsInstalled();
        }

        private async void CheckIsInstalled()
        {
            try
            {
                var manager = await this.packageManagerProvider.GetProxyFor(SelfElevationLevel.AsInvoker).ConfigureAwait(false);
                this.IsInstalled.CurrentValue = await manager.IsInstalled(this.FilePath).ConfigureAwait(false);
            }
            catch
            {
                this.IsInstalled.CurrentValue = false;
                // todo: logging
            }
        }

        public string Title { get; private set; } = "MSIX Hero";
        
        public event Action<IDialogResult> RequestClose;
        
        public string FilePath { get; private set; }
    }
}
