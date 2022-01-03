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
using System.Windows;
using System.Windows.Controls.Primitives;

namespace Otor.MsixHero.App.Controls.PsfContent.View
{
    /// <summary>
    /// Interaction logic for PsfContentView.xaml
    /// </summary>
    public partial class PsfContentView
    {
        public PsfContentView()
        {
            InitializeComponent();
        }

        private void FrameworkElement_OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (!e.WidthChanged)
            {
                return;
            }

            if (e.NewSize.Width < 270 || ((UniformGrid)sender).Children.Count == 1)
            {
                ((UniformGrid) sender).Columns = 1;
            }
            else
            {
                ((UniformGrid) sender).Columns = (int) Math.Floor(e.NewSize.Width / 270.0);
            }
        }
    }
}
