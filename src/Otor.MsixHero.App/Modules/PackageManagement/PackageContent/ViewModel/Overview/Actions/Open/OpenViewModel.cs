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
using System.Threading;
using System.Threading.Tasks;
using Otor.MsixHero.App.Modules.PackageManagement.PackageContent.ViewModel.Common;
using Otor.MsixHero.App.Mvvm;
using Otor.MsixHero.Appx.Packaging.Manifest.Entities;

namespace Otor.MsixHero.App.Modules.PackageManagement.PackageContent.ViewModel.Overview.Actions.Open;

public class OpenViewModel : NotifyPropertyChanged, ILoadPackage
{
    public string RootDirectory { get; private set; }

    public string UserDirectory { get; private set; }

    public Task LoadPackage(AppxPackage model, string filePath, CancellationToken cancellationToken)
    {
        UserDirectory = Path.Combine("%localappdata%", "Packages", model.FamilyName, "LocalCache");

        if (filePath != null)
        {
            RootDirectory = filePath.Replace(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles).TrimEnd('\\'), "%programfiles%");
        }
        else
        {
            RootDirectory = null;
        }

        OnPropertyChanged(nameof(UserDirectory));
        OnPropertyChanged(nameof(RootDirectory));

        return Task.CompletedTask;
    }
}