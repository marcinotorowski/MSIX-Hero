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
using Otor.MsixHero.Appx.Updates.Entities;

namespace Otor.MsixHero.App.Modules.Dialogs.Updates.UpdateImpact.ViewModel.Items
{
    public class FileViewModel : NotifyPropertyChanged
    {
        private FileViewModel(FileType type)
        {
            this.Type = type;
        }

        public FileViewModel(ComparedFile changedFile) : this(FileType.Changed)
        {
            this.Name = changedFile.Name;
            this.UpdateImpact = changedFile.UpdateImpact;
            this.SizeDifference = changedFile.SizeDifference;
        }

        public FileViewModel(FileType type, ComparedFile file) : this(type)
        {
            this.Name = file.Name;
            this.UpdateImpact = file.UpdateImpact;
            this.SizeDifference = type == FileType.Unchanged ? 0 : file.SizeDifference;
        }

        public FileType Type { get; }

        public string Name { get; }

        public long? SizeDifference { get; }

        public long? UpdateImpact { get; }
    }
}
