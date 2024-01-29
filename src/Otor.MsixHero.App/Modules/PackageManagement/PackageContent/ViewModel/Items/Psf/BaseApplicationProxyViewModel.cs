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

using Otor.MsixHero.App.Mvvm;
using Otor.MsixHero.Appx.Psf.Entities.Descriptor;

namespace Otor.MsixHero.App.Modules.PackageManagement.PackageContent.ViewModel.Items.Psf;

public abstract class BaseApplicationProxyViewModel : NotifyPropertyChanged
{
    private readonly BaseApplicationProxy _definition;

    protected BaseApplicationProxyViewModel(BaseApplicationProxy definition)
    {
        this._definition = definition;
    }

    public string Executable => this._definition.Executable;

    public string Arguments => this._definition.Arguments;

    public string WorkingDirectory => this._definition.WorkingDirectory;

    public bool HasArguments => !string.IsNullOrEmpty(this._definition.Arguments);

    public bool HasWorkingDirectory => !string.IsNullOrEmpty(this._definition.WorkingDirectory);

}