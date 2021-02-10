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
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using Otor.MsixHero.Appx.Updates.Entities;
using Otor.MsixHero.Appx.Updates.Entities.Appx;
using Otor.MsixHero.Appx.Updates.Entities.Comparison;

namespace Otor.MsixHero.App.Modules.Dialogs.Updates.UpdateImpact.View
{
    public class PackageBlocksControl : FrameworkElement, IDisposable
    {
        public static readonly DependencyProperty BlocksProperty = DependencyProperty.Register("Blocks", typeof(IList<LayoutBar>), typeof(PackageBlocksControl), new FrameworkPropertyMetadata(null, OnBlocksChanged));
        public static readonly DependencyProperty BrushAddedProperty = DependencyProperty.Register("BrushAdded", typeof(Brush), typeof(PackageBlocksControl), new FrameworkPropertyMetadata(Brushes.ForestGreen, FrameworkPropertyMetadataOptions.AffectsRender));
        public static readonly DependencyProperty BrushDeletedProperty = DependencyProperty.Register("BrushDeleted", typeof(Brush), typeof(PackageBlocksControl), new FrameworkPropertyMetadata(Brushes.Red, FrameworkPropertyMetadataOptions.AffectsRender));
        public static readonly DependencyProperty BrushDuplicatedProperty = DependencyProperty.Register("BrushDuplicated", typeof(Brush), typeof(PackageBlocksControl), new FrameworkPropertyMetadata(Brushes.DodgerBlue, FrameworkPropertyMetadataOptions.AffectsRender));
        public static readonly DependencyProperty BrushChangedProperty = DependencyProperty.Register("BrushChanged", typeof(Brush), typeof(PackageBlocksControl), new FrameworkPropertyMetadata(Brushes.Orange, FrameworkPropertyMetadataOptions.AffectsRender));
        public static readonly DependencyProperty BackgroundProperty = DependencyProperty.Register("Background", typeof(Brush), typeof(PackageBlocksControl), new FrameworkPropertyMetadata(Brushes.LightGray, FrameworkPropertyMetadataOptions.AffectsRender));
        public static readonly DependencyProperty ReferenceLengthProperty = DependencyProperty.Register("ReferenceLength", typeof(long), typeof(PackageBlocksControl), new FrameworkPropertyMetadata(0L, FrameworkPropertyMetadataOptions.AffectsRender));

        private ObservableCollection<LayoutBar> observable;

        ~PackageBlocksControl()
        {
            this.Dispose(false);
        }

        public Brush BrushChanged
        {
            get => (Brush)GetValue(BrushChangedProperty);
            set => SetValue(BrushChangedProperty, value);
        }
        
        public Brush BrushDuplicated
        {
            get => (Brush)GetValue(BrushDuplicatedProperty);
            set => SetValue(BrushDuplicatedProperty, value);
        }
        
        public long ReferenceLength
        {
            get => (long)GetValue(ReferenceLengthProperty);
            set => SetValue(ReferenceLengthProperty, value);
        }

        public Brush BrushAdded
        {
            get => (Brush)GetValue(BrushAddedProperty);
            set => SetValue(BrushAddedProperty, value);
        }

        public Brush BrushDeleted
        {
            get => (Brush)GetValue(BrushDeletedProperty);
            set => SetValue(BrushDeletedProperty, value);
        }
        
        public Brush Background
        {
            get => (Brush)GetValue(BackgroundProperty);
            set => SetValue(BackgroundProperty, value);
        }
        
        public IList<LayoutBar> Blocks
        {
            get => (IList<LayoutBar>)GetValue(BlocksProperty);
            set => SetValue(BlocksProperty, value);
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            base.OnRender(drawingContext);
            var pen = new Pen(Brushes.Transparent, 0);
            if (this.Blocks == null)
            {
                drawingContext.DrawRectangle(this.Background, pen, new Rect(new Point(0, 0), this.RenderSize));
                return;
            }

            var sum = this.Blocks.Last().Position + this.Blocks.Last().Size;
            var totalBlockSize = Math.Max(sum, this.ReferenceLength);
            
            if (totalBlockSize > sum)
            {
                var marginRight = this.RenderSize.Width * sum / totalBlockSize;
                drawingContext.DrawRectangle(this.Background, pen, new Rect(new Point(0, 0), new Size(marginRight, this.RenderSize.Height)));
            }
            else
            {
                drawingContext.DrawRectangle(this.Background, pen, new Rect(new Point(0, 0), this.RenderSize));
            }

            for (var i = 0; i < this.Blocks.Count; i++)
            {
                var block = this.Blocks[i];

                Brush brush;
                
                switch (block.Status)
                {
                    case ComparisonStatus.Changed:
                        brush = this.BrushChanged;
                        break;
                    case ComparisonStatus.Old:
                        brush = this.BrushDeleted;
                        break;
                    case ComparisonStatus.New:
                        brush = this.BrushAdded;
                        break;
                    case ComparisonStatus.Duplicate:
                        brush = this.BrushDuplicated;
                        break;
                    default:
                        continue;
                }

                var beginDraw = new Point(this.RenderSize.Width * block.Position / totalBlockSize, 0);
                var size = new Size(Math.Max(1, this.RenderSize.Width * block.Size / totalBlockSize), this.RenderSize.Height);
                drawingContext.DrawRectangle(brush, pen, new Rect(beginDraw, size));
            }
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (disposing)
            {

            }

            if (this.observable != null)
            {
                this.observable.CollectionChanged -= this.ObservableOnCollectionChanged;
            }
        }

        private void ObservableOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            this.InvalidateVisual();
        }

        private static void OnBlocksChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (e.OldValue is ObservableCollection<LayoutBar> oldObservable)
            {
                var that = (PackageBlocksControl)d;
                oldObservable.CollectionChanged -= that.ObservableOnCollectionChanged;
                that.observable = null;
            }

            if (e.NewValue is ObservableCollection<LayoutBar> observable)
            {
                var that = (PackageBlocksControl) d;
                that.observable = observable;
                observable.CollectionChanged -= that.ObservableOnCollectionChanged;
                observable.CollectionChanged += that.ObservableOnCollectionChanged;
            }
        }
    }
}
