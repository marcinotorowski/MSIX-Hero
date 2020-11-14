using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using Otor.MsixHero.Appx.Updates.Entities.Blocks;

namespace Otor.MsixHero.App.Modules.Editors.Updates.UpdateImpact.View
{
    public class PackageBlocksControl : FrameworkElement, IDisposable
    {
        public static readonly DependencyProperty BlocksProperty = DependencyProperty.Register("Blocks", typeof(IList<Block>), typeof(PackageBlocksControl), new FrameworkPropertyMetadata(null, OnBlocksChanged));
        public static readonly DependencyProperty BrushAddedProperty = DependencyProperty.Register("BrushAdded", typeof(Brush), typeof(PackageBlocksControl), new FrameworkPropertyMetadata(Brushes.ForestGreen, FrameworkPropertyMetadataOptions.AffectsRender));
        public static readonly DependencyProperty BrushDeletedProperty = DependencyProperty.Register("BrushDeleted", typeof(Brush), typeof(PackageBlocksControl), new FrameworkPropertyMetadata(Brushes.Red, FrameworkPropertyMetadataOptions.AffectsRender));
        public static readonly DependencyProperty BrushChangedProperty = DependencyProperty.Register("BrushChanged", typeof(Brush), typeof(PackageBlocksControl), new FrameworkPropertyMetadata(Brushes.Orange, FrameworkPropertyMetadataOptions.AffectsRender));
        public static readonly DependencyProperty BackgroundProperty = DependencyProperty.Register("Background", typeof(Brush), typeof(PackageBlocksControl), new FrameworkPropertyMetadata(Brushes.LightGray, FrameworkPropertyMetadataOptions.AffectsRender));
        public static readonly DependencyProperty ReferenceLengthProperty = DependencyProperty.Register("ReferenceLength", typeof(long), typeof(PackageBlocksControl), new FrameworkPropertyMetadata(0L, FrameworkPropertyMetadataOptions.AffectsRender));

        private ObservableCollection<Block> observable;

        ~PackageBlocksControl()
        {
            this.Dispose(false);
        }

        public Brush BrushChanged
        {
            get => (Brush)GetValue(BrushChangedProperty);
            set => SetValue(BrushChangedProperty, value);
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
        
        public IList<Block> Blocks
        {
            get => (IList<Block>)GetValue(BlocksProperty);
            set => SetValue(BlocksProperty, value);
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            base.OnRender(drawingContext);
            if (this.Blocks == null)
            {
                return;
            }

            var sum = this.Blocks.Sum(b => b.Length);
            var totalBlockSize = Math.Max(sum, this.ReferenceLength);

            var pen = new Pen(Brushes.Transparent, 0);

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

                var allBlocks = this.Blocks.Skip(i).TakeWhile(b => b.BlockType == block.BlockType).Select(b => b.Length).ToList();
                i += allBlocks.Count;

                switch (block.BlockType)
                {
                    case BlockType.Changed:
                        brush = this.BrushChanged;
                        break;
                    case BlockType.Deleted:
                        brush = this.BrushDeleted;
                        break;
                    case BlockType.Added:
                        brush = this.BrushAdded;
                        break;
                    default:
                        continue;
                }

                var beginDraw = new Point(this.RenderSize.Width * block.Position / totalBlockSize, 0);
                var size = new Size(Math.Max(1, this.RenderSize.Width * allBlocks.Sum() / totalBlockSize), this.RenderSize.Height);
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
            if (e.OldValue is ObservableCollection<Block> oldObservable)
            {
                var that = (PackageBlocksControl)d;
                oldObservable.CollectionChanged -= that.ObservableOnCollectionChanged;
                that.observable = null;
            }

            if (e.NewValue is ObservableCollection<Block> observable)
            {
                var that = (PackageBlocksControl) d;
                that.observable = observable;
                observable.CollectionChanged -= that.ObservableOnCollectionChanged;
                observable.CollectionChanged += that.ObservableOnCollectionChanged;
            }
        }
    }
}
