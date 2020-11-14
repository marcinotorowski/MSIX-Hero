using System;
using System.Linq;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;

namespace Otor.MsixHero.App.Controls
{
    public class RequiredAdorner : Adorner
    {
        // Be sure to call the base class constructor.
        public RequiredAdorner(UIElement adornedElement) : base(adornedElement)
        {
        }



        public static bool GetIsRequired(DependencyObject obj)
        {
            return (bool)obj.GetValue(IsRequiredProperty);
        }

        public static void SetIsRequired(DependencyObject obj, bool value)
        {
            obj.SetValue(IsRequiredProperty, value);
        }

        // Using a DependencyProperty as the backing store for IsRequired.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsRequiredProperty = DependencyProperty.RegisterAttached("IsRequired", typeof(bool), typeof(RequiredAdorner), new PropertyMetadata(false, PropertyChangedCallback));

        private static void PropertyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var visual = d as UIElement;
            if (visual == null)
            {
                return;
            }

            var layer = AdornerLayer.GetAdornerLayer(visual);
            if (layer == null)
            {
                return;
            }

            if ((bool) e.NewValue)
            {
                layer.Add(new RequiredAdorner(visual));
            }
            else
            {
                foreach (var adorner in (layer.GetAdorners(visual) ?? Enumerable.Empty<Adorner>()).OfType<RequiredAdorner>().ToArray())
                {
                    layer.Remove(adorner);
                }
            }
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            const int borderWidth = 2;
            const int padding = 2;
            const int radius = 2;

            if (!this.AdornedElement.IsVisible)
            {
                return;
            }


            double width, height;
            if (!double.IsNaN(this.AdornedElement.DesiredSize.Width))
            {
                width = Math.Min(this.AdornedElement.RenderSize.Width, this.AdornedElement.DesiredSize.Width);
            }
            else
            {
                width = this.AdornedElement.RenderSize.Width;
            }

            if (!double.IsNaN(this.AdornedElement.DesiredSize.Height))
            {
                height = Math.Min(this.AdornedElement.RenderSize.Height, this.AdornedElement.DesiredSize.Height);
            }
            else
            {
                height = this.AdornedElement.RenderSize.Height;
            }

            var adornedElementRect = new Rect(new Point(0, 0), new Point(width, height));
            var pointCenter = new Point(adornedElementRect.TopRight.X + padding + radius, adornedElementRect.TopLeft.Y + borderWidth + radius + padding);
            drawingContext.DrawEllipse(Brushes.Gray, new Pen(Brushes.Transparent, 0.0), pointCenter, radius, radius);
        }
    }
}
