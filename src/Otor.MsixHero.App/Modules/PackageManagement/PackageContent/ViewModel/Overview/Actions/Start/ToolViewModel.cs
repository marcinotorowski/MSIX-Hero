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

using System.Windows.Media;
using Otor.MsixHero.App.Helpers.Interop;
using Otor.MsixHero.App.Modules.PackageManagement.PackageContent.ViewModel.Common;
using Otor.MsixHero.App.Mvvm;
using Otor.MsixHero.Infrastructure.Configuration;

namespace Otor.MsixHero.App.Modules.PackageManagement.PackageContent.ViewModel.Overview.Actions.Start
{
    public class ToolViewModel : NotifyPropertyChanged
    {
        public ToolViewModel(ToolListConfiguration model)
        {
            this.Icon = WindowsIcons.GetIconFor(string.IsNullOrEmpty(model.Icon) ? model.Path : model.Icon);
            this.Model = model;
            this.Header = model.Name;
            this.Description = model.Path == null ? null : model.Path;
            this.IsUac = model.AsAdmin;
        }

        public string Header { get; }

        public string Description { get; }

        public bool IsUac { get; }

        public ImageSource Icon { get; }

        public ToolListConfiguration Model { get; }
    }
}