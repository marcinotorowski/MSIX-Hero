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
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace Otor.MsixHero.App.Controls;

public class ButtonRowPanel : Panel
{
    public static readonly DependencyProperty OrderProperty = DependencyProperty.RegisterAttached("Order", typeof(int), typeof(ButtonRowPanel), new PropertyMetadata(0));
    public static readonly DependencyProperty AlignRightProperty = DependencyProperty.RegisterAttached("AlignRight", typeof(bool), typeof(ButtonRowPanel), new PropertyMetadata(false));

    public static bool GetAlignRight(DependencyObject obj)
    {
        return (bool)obj.GetValue(AlignRightProperty);
    }

    public static void SetAlignRight(DependencyObject obj, bool value)
    {
        obj.SetValue(AlignRightProperty, value);
    }
        
    public static int GetOrder(DependencyObject obj)
    {
        return (int)obj.GetValue(OrderProperty);
    }

    public static void SetOrder(DependencyObject obj, int value)
    {
        obj.SetValue(OrderProperty, value);
    }
        
    protected override Size ArrangeOverride(Size finalSize)
    {
        var marginLeft = 0.0;
        var marginRight = 0.0;
        var availableSize = finalSize.Width;

        foreach (UIElement child in this.Children)
        {
            availableSize -= child.DesiredSize.Width;
            if (availableSize < 0)
            {
                break;
            }

            var top = (finalSize.Height - child.DesiredSize.Height) / 2;
            if (!GetAlignRight(child))
            {
                child.Arrange(new Rect(marginLeft, top, child.DesiredSize.Width, top + child.DesiredSize.Height));
                marginLeft += child.DesiredSize.Width;
            }
            else
            {
                child.Arrange(new Rect(finalSize.Width - marginRight - child.DesiredSize.Width, top, child.DesiredSize.Width, top + child.DesiredSize.Height));
                marginRight += child.DesiredSize.Width;
            }
        }

        return finalSize;
    }

    protected override Size MeasureOverride(Size availableSize)
    {
        var totalSize = new Size();

        foreach (UIElement child in this.Children)
        {
            child.Measure(availableSize);
            totalSize.Width += child.DesiredSize.Width;
            totalSize.Height = Math.Max(totalSize.Height, child.DesiredSize.Height);
        }

        if (totalSize.Width > availableSize.Width)
        {
            var orderedChildren = this.Children.OfType<UIElement>().OrderByDescending(GetOrder);
            // we need to recalculate
            foreach (var child in orderedChildren)
            {
                var currentWidth = child.DesiredSize.Width;

                // Note: The parameter 60 is one that works the best for the current implementation.
                // This probably should be a DP to be more generic, but for a time being in does its purpose.
                child.Measure(new Size(60, totalSize.Height));
                totalSize.Width -= currentWidth;
                totalSize.Width += child.DesiredSize.Width;

                if (totalSize.Width < availableSize.Width)
                {
                    break;
                }
            }
        }

        return totalSize;
    }
}