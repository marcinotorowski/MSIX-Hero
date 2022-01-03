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

using Otor.MsixHero.App.Mvvm;

namespace Otor.MsixHero.App.Modules.Dialogs.Packaging.Pack.ViewModel
{
    public interface ISelectableItem
    {
        bool IsChecked { get; set; }

        string DisplayContent { get; }
    }

    public class SelectableItem<T> : NotifyPropertyChanged, ISelectableItem
    {
        private bool isChecked;

        public SelectableItem(T value, string displayContent, bool isChecked = false)
        {
            Value = value;
            this.isChecked = isChecked;
            this.DisplayContent = displayContent;
        }

        public T Value { get; }

        public bool IsChecked
        {
            get => isChecked;
            set => this.SetField(ref this.isChecked, value);
        }

        public string DisplayContent { get; }
    }
}