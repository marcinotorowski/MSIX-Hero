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

using Otor.MsixHero.App.Helpers;

namespace Otor.MsixHero.App.Mvvm.Changeable.Dialog.ViewModel
{
    public class DialogState : NotifyPropertyChanged
    {
        private bool isSaved;
        private bool wasSaved;
        
        public bool IsSaved
        {
            get => this.isSaved;
            set => this.SetField(ref this.isSaved, value);
        }

        public bool WasSaved
        {
            get => this.IsSaved || this.wasSaved;
            set => this.SetField(ref this.wasSaved, value);
        }

        public ProgressProperty Progress { get; } = new ProgressProperty();
    }
}