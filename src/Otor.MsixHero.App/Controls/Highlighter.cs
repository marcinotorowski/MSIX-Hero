// MSIX Hero
// Copyright (C) 2024 Marcin Otorowski
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
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

namespace Otor.MsixHero.App.Controls
{
    internal class Highlighter : DependencyObject
    {
        public static readonly DependencyProperty SelectionProperty = DependencyProperty.RegisterAttached("Selection", typeof(string), typeof(Highlighter), new PropertyMetadata(SelectText));

        public static string GetSelection(DependencyObject obj)
        {
            return (string)obj.GetValue(SelectionProperty);
        }

        public static void SetSelection(DependencyObject obj, string value)
        {
            obj.SetValue(SelectionProperty, value);
        }

        private static void SelectText(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (!(d is TextBlock textBlock) || textBlock == null)
            {
                throw new InvalidOperationException("Only valid for TextBlock");
            }

            var text = textBlock.Text;
            if (textBlock.Inlines.Any())
            {
                textBlock.Inlines.Clear();
            }

            if (string.IsNullOrEmpty(text))
            {
                return;
            }

            var highlightText = (string)d.GetValue(SelectionProperty);
            if (string.IsNullOrEmpty(highlightText))
            {
                textBlock.Inlines.Add(new Run(text));
                return;
            }

            var index = text.IndexOf(highlightText, StringComparison.CurrentCultureIgnoreCase);
            if (index < 0)
            {
                textBlock.Inlines.Add(new Run(text));
                return;
            }

            var selectionColor = (Brush)d.GetValue(HighlightColorProperty);
            var foreColor = (Brush)d.GetValue(ForecolorProperty);

            while (true)
            {
                if (index != 0)
                {
                    textBlock.Inlines.Add(new Run(text.Substring(0, index)));
                }

                textBlock.Inlines.Add(
                    new Run(text.Substring(index, highlightText.Length)) {
                        Background = selectionColor,
                        Foreground = foreColor}
                );

                text = text.Substring(index + highlightText.Length);
                index = text.IndexOf(highlightText, StringComparison.CurrentCultureIgnoreCase);

                if (index < 0)
                {
                    textBlock.Inlines.Add(new Run(text));
                    break;
                }
            }
        }

        public static Brush GetHighlightColor(DependencyObject obj)
        {
            return (Brush)obj.GetValue(HighlightColorProperty);
        }

        public static void SetHighlightColor(DependencyObject obj, Brush value)
        {
            obj.SetValue(HighlightColorProperty, value);
        }

        public static readonly DependencyProperty HighlightColorProperty = DependencyProperty.RegisterAttached("HighlightColor", typeof(Brush), typeof(Highlighter), new PropertyMetadata(new SolidColorBrush(Color.FromArgb(255, 255, 238, 128)), SelectText));


        public static Brush GetForecolor(DependencyObject obj)
        {
            return (Brush)obj.GetValue(ForecolorProperty);
        }

        public static void SetForecolor(DependencyObject obj, Brush value)
        {
            obj.SetValue(ForecolorProperty, value);
        }

        public static readonly DependencyProperty ForecolorProperty = DependencyProperty.RegisterAttached("Forecolor", typeof(Brush), typeof(Highlighter), new PropertyMetadata(Brushes.Black, SelectText));
    }
}
