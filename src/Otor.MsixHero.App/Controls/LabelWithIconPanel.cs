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
using System.Windows;
using System.Windows.Controls;

namespace Otor.MsixHero.App.Controls;

public class LabelWithIconPanel : Panel
{
    protected override Size ArrangeOverride(Size finalSize)
    {
        var size = new Size();
        Size firstChild = default;

        foreach (UIElement child in this.Children)
        {
            size.Height = Math.Max(size.Height, child.DesiredSize.Height);

            if (firstChild == default)
            {
                firstChild = child.DesiredSize;
            }
            else if (size.Width + child.DesiredSize.Width > finalSize.Width)
            {
                size.Width += child.DesiredSize.Width;
                child.Arrange(new Rect(0, 0, 0, 0));
                break;
            }

            var requiredHeight = child.DesiredSize.Height;
            var top = (finalSize.Height - requiredHeight) / 2;
            child.Arrange(new Rect(size.Width, top, child.DesiredSize.Width, child.DesiredSize.Height));
            size.Width += child.DesiredSize.Width;
        }

        return size;
    }

    protected override Size MeasureOverride(Size availableSize)
    {
        if (this.Children.Count == 0)
        {
            return availableSize;
        }

        var size = new Size();
        Size firstChild = default;

        var sizeToMeasure = new Size(double.PositiveInfinity, double.PositiveInfinity);

        foreach (UIElement child in this.Children)
        {
            child.Measure(sizeToMeasure);
            if (firstChild == default)
            {
                firstChild = child.DesiredSize;
            }

            size.Width += child.DesiredSize.Width;
            size.Height = Math.Max(size.Height, child.DesiredSize.Height);
        }

        if (size.Width > availableSize.Width)
        {
            return firstChild;
        }

        return size;
    }
}