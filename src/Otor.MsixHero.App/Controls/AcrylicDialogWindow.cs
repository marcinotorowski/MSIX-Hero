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

using System.Windows.Media;
using Prism.Services.Dialogs;
using SourceChord.FluentWPF;

namespace Otor.MsixHero.App.Controls
{
    public class AcrylicDialogWindow : AcrylicWindow, IDialogWindow
    {
        public AcrylicDialogWindow()
        {
            this.ExtendViewIntoTitleBar = true;
            // ReSharper disable once PossibleNullReferenceException
            this.TintColor = (Color)ColorConverter.ConvertFromString("#0173C7");
            this.TintOpacity = 0.2;
            this.FallbackColor = Color.FromRgb(204, 204, 204);
        }

        public IDialogResult Result { get; set; }
    }

}
