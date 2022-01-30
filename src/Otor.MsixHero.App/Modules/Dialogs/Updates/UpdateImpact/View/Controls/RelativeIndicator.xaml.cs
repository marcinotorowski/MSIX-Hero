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
using System.Windows.Media;

namespace Otor.MsixHero.App.Modules.Dialogs.Updates.UpdateImpact.View.Controls
{
    /// <summary>
    /// Interaction logic for RelativeIndicator.
    /// </summary>
    public partial class RelativeIndicator
    {
        public static readonly DependencyProperty OldValueProperty = DependencyProperty.Register("OldValue", typeof(double), typeof(RelativeIndicator), new PropertyMetadata(0.0, PropertyChangedCallback));
        
        public static readonly DependencyProperty NewValueProperty = DependencyProperty.Register("NewValue", typeof(double), typeof(RelativeIndicator), new PropertyMetadata(0.0, PropertyChangedCallback));

        public static readonly DependencyProperty IsReversedProperty = DependencyProperty.Register("IsReversed", typeof(bool), typeof(RelativeIndicator), new PropertyMetadata(false, PropertyChangedCallback));

        public RelativeIndicator()
        {
            this.InitializeComponent();
        }

        public bool IsReversed
        {
            get => (bool)GetValue(IsReversedProperty);
            set => SetValue(IsReversedProperty, value);
        }

        public double OldValue
        {
            get => (double)GetValue(OldValueProperty);
            set => SetValue(OldValueProperty, value);
        }

        public double NewValue
        {
            get => (double)GetValue(NewValueProperty);
            set => SetValue(NewValueProperty, value);
        }
        
        private static void PropertyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var that = (RelativeIndicator) d;
            if (Math.Abs(that.OldValue) < 0.1)
            {
                that.PART_Text.Text = MsixHero.App.Resources.Localization.Dialogs_UpdateImpact_SizeNoDiff;
                that.PART_Icon.Visibility = Visibility.Collapsed;
                return;
            }

            if (that.OldValue < that.NewValue)
            {
                that.PART_Text.Text = $"+{Math.Round(100.0 * (that.NewValue - that.OldValue) / that.OldValue, 2):0.00}%";
                that.PART_Icon.Visibility = Visibility.Visible;
                that.PART_Icon.Fill = that.IsReversed ? Brushes.Red : Brushes.Green;
                ((RotateTransform)that.PART_Icon.RenderTransform).Angle = 0;
                return;
            }

            that.PART_Text.Text = $"-{Math.Round(100.0 * (that.OldValue - that.NewValue) / that.OldValue, 2):0.00}%";
            that.PART_Icon.Visibility = Visibility.Visible;
            that.PART_Icon.Fill = that.IsReversed ? Brushes.Green : Brushes.Red;
            ((RotateTransform) that.PART_Icon.RenderTransform).Angle = 180;
        }
    }
}
