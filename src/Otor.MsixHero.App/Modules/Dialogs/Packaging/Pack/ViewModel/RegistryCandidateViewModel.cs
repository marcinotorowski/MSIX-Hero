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

using System.IO;
using Otor.MsixHero.App.Mvvm;

namespace Otor.MsixHero.App.Modules.Dialogs.Packaging.Pack.ViewModel
{
    public class RegistryCandidateViewModel : NotifyPropertyChanged
    {
        public RegistryCandidateViewModel(string filePath, string displayContent)
        {
            DisplayContent = displayContent;
            FilePath = filePath;
        }

        public RegistryCandidateViewModel(string filePath) : this(filePath, Path.GetFileName(filePath))
        {
        }

        public RegistryCandidateViewModel() : this(null, "No conversion")
        {
        }

        public string DisplayContent { get; private set; }

        public string FilePath { get; private set; }
    }
}