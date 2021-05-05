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

using Otor.MsixHero.App.Mvvm;

namespace Otor.MsixHero.App.Controls.PackageExpert.ViewModels.Items.Content
{
    public abstract class TreeNodeViewModel : NotifyPropertyChanged, ISelectableItem
    {
        private bool isSelected;
        
        public string Path { get; protected set; }

        public string Name { get; protected set; }
        
        public bool IsSelected
        {
            get => this.isSelected;
            set => this.SetField(ref this.isSelected, value);
        }
    }
}