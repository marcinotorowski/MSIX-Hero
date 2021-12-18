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

using System;
using System.Globalization;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace Otor.MsixHero.App.Modules.Dialogs.Updates.UpdateImpact.View.Controls
{
    public class CustomImpactControl : FrameworkElement
    {
        private Storyboard currentAnimation;

        public static readonly DependencyProperty OldTotalSizeProperty = DependencyProperty.Register("OldTotalSize", typeof(long), typeof(CustomImpactControl), new FrameworkPropertyMetadata(0L, FrameworkPropertyMetadataOptions.AffectsRender, PropertyChangedCallback));

        public static readonly DependencyProperty NewTotalSizeProperty = DependencyProperty.Register("NewTotalSize", typeof(long), typeof(CustomImpactControl), new FrameworkPropertyMetadata(0L, FrameworkPropertyMetadataOptions.AffectsRender, PropertyChangedCallback));
        public static readonly DependencyProperty ChangedSizeNewProperty = DependencyProperty.Register("ChangedSizeNew", typeof(long), typeof(CustomImpactControl), new FrameworkPropertyMetadata(0L, FrameworkPropertyMetadataOptions.AffectsRender, PropertyChangedCallback));
        public static readonly DependencyProperty ChangedSizeOldProperty = DependencyProperty.Register("ChangedSizeOld", typeof(long), typeof(CustomImpactControl), new FrameworkPropertyMetadata(0L, FrameworkPropertyMetadataOptions.AffectsRender, PropertyChangedCallback));
        public static readonly DependencyProperty AddedSizeProperty = DependencyProperty.Register("AddedSize", typeof(long), typeof(CustomImpactControl), new FrameworkPropertyMetadata(0L, FrameworkPropertyMetadataOptions.AffectsRender, PropertyChangedCallback));
        public static readonly DependencyProperty DeletedSizeProperty = DependencyProperty.Register("DeletedSize", typeof(long), typeof(CustomImpactControl), new FrameworkPropertyMetadata(0L, FrameworkPropertyMetadataOptions.AffectsRender, PropertyChangedCallback));
        public static readonly DependencyProperty UnchangedSizeProperty = DependencyProperty.Register("UnchangedSize", typeof(long), typeof(CustomImpactControl), new FrameworkPropertyMetadata(0L, FrameworkPropertyMetadataOptions.AffectsRender, PropertyChangedCallback));
        public static readonly DependencyProperty AnimationProgressProperty = DependencyProperty.Register("AnimationProgress", typeof(double), typeof(CustomImpactControl), new FrameworkPropertyMetadata(1.0, FrameworkPropertyMetadataOptions.AffectsRender));

        public long OldTotalSize
        {
            get => (long)this.GetValue(OldTotalSizeProperty);
            set => this.SetValue(OldTotalSizeProperty, value);
        }

        public double AnimationProgress
        {
            get => (double)this.GetValue(AnimationProgressProperty);
            set => this.SetValue(AnimationProgressProperty, value);
        }

        public long NewTotalSize
        {
            get => (long)this.GetValue(NewTotalSizeProperty);
            set => this.SetValue(NewTotalSizeProperty, value);
        }
        public long ChangedSizeNew
        {
            get => (long)this.GetValue(ChangedSizeNewProperty);
            set => this.SetValue(ChangedSizeNewProperty, value);
        }
        public long ChangedSizeOld
        {
            get => (long)this.GetValue(ChangedSizeOldProperty);
            set => this.SetValue(ChangedSizeOldProperty, value);
        }
        public long AddedSize
        {
            get => (long)this.GetValue(AddedSizeProperty);
            set => this.SetValue(AddedSizeProperty, value);
        }

        public long DeletedSize
        {
            get => (long)this.GetValue(DeletedSizeProperty);
            set => this.SetValue(DeletedSizeProperty, value);
        }
        public long UnchangedSize
        {
            get => (long)this.GetValue(UnchangedSizeProperty);
            set => this.SetValue(UnchangedSizeProperty, value);
        }

        private static void PropertyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var that = (CustomImpactControl)d;
            if (that.currentAnimation != null)
            {
                that.currentAnimation.Stop();
                that.currentAnimation = null;
            }

            var storyboard = new Storyboard();
            storyboard.Children.Add(new DoubleAnimation(0.0, 1.0, new Duration(TimeSpan.FromSeconds(1)))
            {
                AccelerationRatio = 0.5,
                DecelerationRatio = 0.5,
            });

            Storyboard.SetTarget(storyboard, that);
            Storyboard.SetTargetProperty(storyboard, new PropertyPath(nameof(CustomImpactControl.AnimationProgressProperty)));
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            base.OnRender(drawingContext);

            var textPadding = new Thickness(4, 2, 4, 2);
            var textMargin = new Thickness(5, 5, 0, 5);

#pragma warning disable CS0618
            var textRemoved = new FormattedText("Removed files (bytes)", CultureInfo.CurrentUICulture, FlowDirection.LeftToRight, new Typeface("Segoe UI"), 10, Brushes.White);
            var textAdded = new FormattedText("Added files (bytes)", CultureInfo.CurrentUICulture, FlowDirection.LeftToRight, new Typeface("Segoe UI"), 10, Brushes.White);
            var textChanged = new FormattedText("Changed files (bytes)", CultureInfo.CurrentUICulture, FlowDirection.LeftToRight, new Typeface("Segoe UI"), 10, Brushes.White);
            var textUnchanged = new FormattedText("Unchanged files (bytes)", CultureInfo.CurrentUICulture, FlowDirection.LeftToRight, new Typeface("Segoe UI"), 10, Brushes.Black);
#pragma warning restore CS0618

            var textWidth = textRemoved.Width;
            var textHeight = textRemoved.Height;

            var totalWidth = this.RenderSize.Width;
            var totalHeight = this.RenderSize.Height; // reserve 20 for rooter

            var labelHeightWithPaddingAndMargin = textHeight + textPadding.Top + textPadding.Bottom + textMargin.Bottom + textMargin.Top;

            var totalSizeForFullWidth = (double)this.DeletedSize + Math.Max(this.NewTotalSize, this.OldTotalSize);

            var widthRemoved = (this.DeletedSize / totalSizeForFullWidth) * totalWidth;
            var widthUnchanged = (this.UnchangedSize / totalSizeForFullWidth) * totalWidth;
            var widthChangedOld = (this.ChangedSizeOld / totalSizeForFullWidth) * totalWidth;
            var widthChangedNew = (this.ChangedSizeNew / totalSizeForFullWidth) * totalWidth;
            var widthNewFiles = totalSizeForFullWidth - widthRemoved - widthUnchanged - widthChangedNew;

            var totalWidthForAnimation = widthRemoved + widthUnchanged * 2 + widthChangedNew + widthChangedOld + widthNewFiles;

            var thresholdRemoved = widthRemoved / totalWidthForAnimation;
            var thresholdUnchanged1 = widthUnchanged / totalWidthForAnimation;
            var thresholdChangedOld = widthChangedOld / totalWidthForAnimation;
            var thresholdChangedNew = widthChangedNew / totalWidthForAnimation;
            var thresholdUnchanged2 = widthUnchanged / totalWidthForAnimation;
            var thresholdNewFiles = widthNewFiles / totalWidthForAnimation;

            var startRemoved = 0;
            var startUnchanged1 = startRemoved + thresholdRemoved;
            var startChangedOld = startUnchanged1 + thresholdUnchanged1;
            var startUnchanged2 = startChangedOld + thresholdChangedOld;
            var startChangedNew = startUnchanged2 + thresholdUnchanged2;
            var startAdded = startChangedNew + thresholdChangedNew;

            var firstRow = 0;
            var rowHeight = (totalHeight - 2 * labelHeightWithPaddingAndMargin) / 2;
            var secondRow = rowHeight + labelHeightWithPaddingAndMargin;

            var transparentPen = new Pen(Brushes.Transparent, 0);
            var linePen = new Pen(Brushes.OrangeRed, 1);

            drawingContext.DrawLine(linePen, new Point(widthRemoved + widthUnchanged + 1, firstRow + rowHeight), new Point(widthRemoved + widthUnchanged + 1, secondRow));
            drawingContext.DrawLine(linePen, new Point(widthRemoved + widthUnchanged + widthChangedOld - 1, firstRow + rowHeight), new Point(widthRemoved + widthUnchanged + widthChangedNew - 1, secondRow));

            if (this.AnimationProgress >= startRemoved)
            {
                drawingContext.DrawRectangle(Brushes.IndianRed, transparentPen, new Rect(new Point(0, firstRow), new Size(widthRemoved, rowHeight)));
            }

            if (this.AnimationProgress >= startUnchanged1)
            {
                drawingContext.DrawRectangle(Brushes.LightGray, transparentPen, new Rect(new Point(widthRemoved, firstRow), new Size(widthUnchanged, rowHeight)));
            }

            if (this.AnimationProgress >= startChangedNew)
            {
                drawingContext.DrawRectangle(Brushes.DarkOrange, transparentPen, new Rect(new Point(widthRemoved + widthUnchanged, firstRow), new Size(widthChangedOld, rowHeight)));
            }

            if (this.AnimationProgress >= startUnchanged2)
            {
                drawingContext.DrawRectangle(Brushes.LightGray, transparentPen, new Rect(new Point(widthRemoved, secondRow), new Size(widthUnchanged, rowHeight)));
            }

            if (this.AnimationProgress >= startChangedNew)
            {
                drawingContext.DrawRectangle(Brushes.DarkOrange, transparentPen, new Rect(new Point(widthRemoved + widthUnchanged, secondRow), new Size(widthChangedNew, rowHeight)));
            }

            if (this.AnimationProgress >= startAdded)
            {
                drawingContext.DrawRectangle(Brushes.DarkSeaGreen, transparentPen, new Rect(new Point(widthRemoved + widthUnchanged + widthChangedNew, secondRow), new Size(widthNewFiles, rowHeight)));
            }

            if (this.DeletedSize > 0)
            {
                drawingContext.DrawRectangle(Brushes.IndianRed, transparentPen, new Rect(new Point(0, firstRow + rowHeight + textMargin.Top), new Size(textWidth + textPadding.Left + textPadding.Right, textHeight + textPadding.Top + textPadding.Bottom)));
                drawingContext.DrawText(textRemoved, new Point(textPadding.Left, firstRow + rowHeight + textMargin.Top + textPadding.Top));
            }

            var reservedBottom = 0.0;
            if (this.AddedSize > 0)
            {
                drawingContext.DrawRectangle(Brushes.DarkSeaGreen, transparentPen, new Rect(new Point(totalWidth - textAdded.Width - textPadding.Left - textPadding.Right, secondRow + rowHeight + textMargin.Top), new Size(textAdded.Width + textPadding.Left + textPadding.Right, textAdded.Height + textPadding.Top + textPadding.Bottom)));
                drawingContext.DrawText(textAdded, new Point(totalWidth - textAdded.Width - textPadding.Left, secondRow + rowHeight + textMargin.Top + textPadding.Top));
                reservedBottom += textAdded.Width + textPadding.Left + textPadding.Right + textMargin.Left + textMargin.Right;
            }

            if (this.ChangedSizeOld > 0 || this.ChangedSizeNew > 0)
            {
                drawingContext.DrawRectangle(Brushes.DarkOrange, transparentPen, new Rect(new Point(totalWidth - textChanged.Width - textPadding.Left - textPadding.Right - reservedBottom, secondRow + rowHeight + textMargin.Top), new Size(textChanged.Width + textPadding.Left + textPadding.Right, textChanged.Height + textPadding.Top + textPadding.Bottom)));
                drawingContext.DrawText(textChanged, new Point(totalWidth - reservedBottom - textChanged.Width - textPadding.Left, secondRow + rowHeight + textMargin.Top + textPadding.Top));
                reservedBottom += textChanged.Width + textPadding.Left + textPadding.Right + textMargin.Left + textMargin.Right;
            }


            if (this.UnchangedSize > 0)
            {
                var middle = (widthRemoved + totalWidth - reservedBottom) / 2;
                var begin = middle - textUnchanged.Width / 2 - textPadding.Left - textMargin.Left;
                if (begin < widthRemoved)
                {
                    begin = widthRemoved;
                }

                if (begin + textUnchanged.Width + textPadding.Left + textPadding.Right + textMargin.Left + textMargin.Right > totalWidth - reservedBottom)
                {
                    begin = totalWidth - reservedBottom - textUnchanged.Width + textPadding.Left + textPadding.Right - (textMargin.Left + textMargin.Right);
                }

                drawingContext.DrawRectangle(Brushes.LightGray, transparentPen, new Rect(begin, secondRow + rowHeight + textMargin.Top, textUnchanged.Width + textPadding.Left + textPadding.Right, textUnchanged.Height + textPadding.Top + textPadding.Bottom));
                drawingContext.DrawText(textUnchanged, new Point(begin + textPadding.Left, secondRow + rowHeight + textMargin.Top + textPadding.Top));
            }
        }
    }
}
