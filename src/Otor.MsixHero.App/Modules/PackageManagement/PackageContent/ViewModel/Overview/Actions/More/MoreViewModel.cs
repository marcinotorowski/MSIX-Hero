// MSIX Hero
// Copyright (C) 2024 Marcin Otorowski
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

using System.Threading;
using System.Threading.Tasks;
using Otor.MsixHero.App.Modules.PackageManagement.PackageContent.ViewModel.Common;
using Otor.MsixHero.App.Mvvm;
using Otor.MsixHero.Appx.Packaging;
using Otor.MsixHero.Appx.Reader.Manifest.Entities;

namespace Otor.MsixHero.App.Modules.PackageManagement.PackageContent.ViewModel.Overview.Actions.More;

public class MoreViewModel : NotifyPropertyChanged, ILoadPackage, IInstallationAware
{
    private bool _isInstalled;

    public bool IsLoading => false;

    public Task LoadPackage(AppxPackage model, PackageEntry installEntry, string filePath, CancellationToken cancellationToken)
    {
        if (installEntry != null)
        {
            this._isInstalled = true;
        }

        return Task.CompletedTask;
    }

    public bool IsInstalled
    {
        get => this._isInstalled;
        set => this.SetField(ref this._isInstalled, value);
    }
}