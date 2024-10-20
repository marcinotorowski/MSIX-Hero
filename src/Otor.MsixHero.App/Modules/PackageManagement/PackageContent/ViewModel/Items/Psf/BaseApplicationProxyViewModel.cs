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
using Otor.MsixHero.Appx.Reader.Psf.Entities.Descriptor;

namespace Otor.MsixHero.App.Modules.PackageManagement.PackageContent.ViewModel.Items.Psf;

public abstract class BaseApplicationProxyViewModel(BaseApplicationProxy definition) : NotifyPropertyChanged
{
    public string Executable => definition.Executable;

    public string Arguments => definition.Arguments;

    public string WorkingDirectory => definition.WorkingDirectory;

    public bool HasArguments => !string.IsNullOrEmpty(definition.Arguments);

    public bool HasWorkingDirectory => !string.IsNullOrEmpty(definition.WorkingDirectory);

}