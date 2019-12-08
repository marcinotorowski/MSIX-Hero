using System;
using System.Linq;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;

namespace otor.msixhero.ui.Helpers
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

            var adornedElementRect = new Rect(this.AdornedElement.RenderSize);
            var pointCenter = new Point(adornedElementRect.TopRight.X + padding + radius, adornedElementRect.TopLeft.Y + borderWidth + radius + padding);
            drawingContext.DrawEllipse(Brushes.Gray, new Pen(Brushes.Transparent, 0.0), pointCenter, radius, radius);
        }
    }
}
