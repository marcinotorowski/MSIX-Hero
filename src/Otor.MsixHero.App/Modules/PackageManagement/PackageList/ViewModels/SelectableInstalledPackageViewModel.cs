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

using Otor.MsixHero.App.Hero.Commands.Packages;
using Otor.MsixHero.App.Hero.Executor;
using Otor.MsixHero.Appx.Packaging.Installation.Entities;

namespace Otor.MsixHero.App.Modules.PackageManagement.PackageList.ViewModels;

public class SelectableInstalledPackageViewModel : InstalledPackageViewModel
{
    private readonly IMsixHeroCommandExecutor _commandExecutor;
    private bool _isSelected;
    private bool _hasStar;

    public SelectableInstalledPackageViewModel(InstalledPackage package, IMsixHeroCommandExecutor commandExecutor, bool isSelected = false, bool star = false) : base(package)
    {
        this._commandExecutor = commandExecutor;
        this._isSelected = isSelected;
        this._hasStar = star;
    }

    public bool HasStar
    {
        get => this._hasStar;
        set
        {
            if (this.SetField(ref this._hasStar, value))
            {
                if (value)
                {
                    this._commandExecutor.Invoke(this, new StarPackageCommand(this.PackageFullName));
                }
                else
                {
                    this._commandExecutor.Invoke(this, new UnstarPackageCommand(this.PackageFullName));
                }
            }
        }
    }

    public bool IsSelected
    {
        get => this._isSelected;
        set
        {
            if (!this.SetField(ref _isSelected, value))
            {
                return;
            }

            // ReSharper disable once ConvertIfStatementToConditionalTernaryExpression
            if (value)
            {
                this._commandExecutor.Invoke(this, new SelectPackagesCommand(this.PackageFullName, SelectPackagesCommand.PackageSelectionMode.Add));
            }
            else
            {
                this._commandExecutor.Invoke(this, new SelectPackagesCommand(this.PackageFullName, SelectPackagesCommand.PackageSelectionMode.Remove));
            }
        }
    }
}