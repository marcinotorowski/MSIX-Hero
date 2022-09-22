using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Otor.MsixHero.App.Controls
{
    public class CircularProgress : Shape
    {
        public static readonly DependencyProperty ValueProperty = DependencyProperty.Register(nameof(Value), typeof(double), typeof(CircularProgress), new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.AffectsRender, null, CoerceValue));

        public static readonly DependencyProperty InitialAngleProperty = DependencyProperty.Register(nameof(InitialAngle), typeof(double), typeof(CircularProgress), new FrameworkPropertyMetadata(90.0, FrameworkPropertyMetadataOptions.AffectsRender, null, CoerceValue));
        
        public double InitialAngle
        {
            get => (double)GetValue(InitialAngleProperty);
            set => SetValue(InitialAngleProperty, value);
        }
        
        public double Value
        {
            get => (double)GetValue(ValueProperty);
            set => SetValue(ValueProperty, value);
        }
        
        private static object CoerceValue(DependencyObject depObj, object baseVal)
        {
            var val = (double)baseVal;
            val = Math.Min(val, 99.99999999);
            val = Math.Max(val, 0.0);
            return val;
        }

        protected override Geometry DefiningGeometry
        {
            get
            {
                var endAngle = this.InitialAngle - this.Value / 100.0 * 360.0;

                var maxWidth = Math.Max(0.0, this.Width - this.StrokeThickness);
                var maxHeight = Math.Max(0.0, this.Height - this.StrokeThickness);

                var xStart = maxWidth / 2.0 * Math.Cos(this.InitialAngle * Math.PI / 180.0);
                var yStart = maxHeight / 2.0 * Math.Sin(this.InitialAngle * Math.PI / 180.0);

                var xEnd = maxWidth / 2.0 * Math.Cos(endAngle * Math.PI / 180.0);
                var yEnd = maxHeight / 2.0 * Math.Sin(endAngle * Math.PI / 180.0);

                var geom = new StreamGeometry();
                using var ctx = geom.Open();
                ctx.BeginFigure(new Point(this.Width / 2.0 + xStart, this.Height / 2.0 - yStart), true, false);
                ctx.ArcTo(
                    new Point(
                        this.Width / 2.0 + xEnd,
                        this.Height / 2.0 - yEnd), 
                    new Size(maxWidth / 2.0, maxHeight / 2),
                    0.0,
                    this.InitialAngle - endAngle > 180,
                    SweepDirection.Clockwise,
                    true,
                    false);

                return geom;
            }
        }
    }
}
