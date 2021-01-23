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

using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace Otor.MsixHero.App.Controls.PackageExpert.Views
{
    /// <summary>
    /// Interaction logic for ActionBar.
    /// </summary>
    public partial class ActionBar
    {
        public static readonly DependencyProperty ToolsProperty = DependencyProperty.Register("Tools", typeof(ObservableCollection<ToolItem>), typeof(ActionBar), new PropertyMetadata(null));

        public ActionBar()
        {
            InitializeComponent();
            this.PopupMore.CustomPopupPlacementCallback = this.CustomPopupPlacementCallback;
        }

        private CustomPopupPlacement[] CustomPopupPlacementCallback(Size popupSize, Size targetSize, Point offset)
        {
            return new[]
            {
                new CustomPopupPlacement(new Point(offset.X - popupSize.Width + targetSize.Width, offset.Y + targetSize.Height), PopupPrimaryAxis.Vertical) 
            };
        }

        public ObservableCollection<ToolItem> Tools 
        {
            get => (ObservableCollection<ToolItem>)GetValue(ToolsProperty);
            set => SetValue(ToolsProperty, value);
        }
    }
}
