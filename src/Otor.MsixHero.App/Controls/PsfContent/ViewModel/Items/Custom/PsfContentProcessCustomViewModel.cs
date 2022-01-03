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

using System.Text.RegularExpressions;
using Otor.MsixHero.App.Mvvm.Changeable;
using Otor.MsixHero.Appx.Psf.Entities;

namespace Otor.MsixHero.App.Controls.PsfContent.ViewModel.Items.Custom
{
    public class PsfContentProcessCustomViewModel : PsfContentProcessViewModel
    {
        private readonly ChangeableProperty<string> json;

        public PsfContentProcessCustomViewModel(string processRegularExpression, string fixupName, CustomPsfFixupConfig customFixup) : base(processRegularExpression, fixupName)
        {
            this.json = new ChangeableProperty<string>(customFixup.Json);
            this.AddChild(this.json);
            this.Header = Regex.Replace(fixupName, @"[x_]*(?:64|86|32)?(?:\.dll)?$", string.Empty).ToUpperInvariant();
        }

        public string Header { get; }

        public string Json
        {
            get => this.json.CurrentValue;
            set
            {
                if (this.json.CurrentValue == value)
                {
                    return;
                }

                this.json.CurrentValue = value;
                this.OnPropertyChanged();
            }
        }
    }
}
