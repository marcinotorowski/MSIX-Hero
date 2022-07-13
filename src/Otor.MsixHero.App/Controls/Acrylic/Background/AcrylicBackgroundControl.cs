using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Otor.MsixHero.App.Controls.Acrylic.Background
{
    public class AcrylicBackgroundControl : Control
    {
        public static readonly DependencyProperty TintTextProperty = DependencyProperty.Register("TintText", typeof(string), typeof(AcrylicBackgroundControl), new PropertyMetadata(default(string)));

        public static readonly DependencyProperty TintBrushProperty = DependencyProperty.Register("TintBrush", typeof(Brush), typeof(AcrylicBackgroundControl), new PropertyMetadata(default(Brush)));

        public static readonly DependencyProperty TintContentProperty = DependencyProperty.Register("TintContent", typeof(object), typeof(AcrylicBackgroundControl), new PropertyMetadata(default(object)));

        public static readonly DependencyProperty NoiseOpacityProperty = DependencyProperty.Register("NoiseOpacity", typeof(double), typeof(AcrylicBackgroundControl), new PropertyMetadata(default(double)));

        public static readonly DependencyProperty TintTextOpacityProperty = DependencyProperty.Register("TintTextOpacity", typeof(double), typeof(AcrylicBackgroundControl), new PropertyMetadata(default(double)));
        
        public static readonly DependencyProperty TintOpacityProperty = DependencyProperty.Register("TintOpacity", typeof(double), typeof(AcrylicBackgroundControl), new PropertyMetadata(default(double)));
        
        public double TintTextOpacity
        {
            get => (double) GetValue(TintTextOpacityProperty);
            set => SetValue(TintTextOpacityProperty, value);
        }
        
        public double TintOpacity
        {
            get => (double) GetValue(TintOpacityProperty);
            set => SetValue(TintOpacityProperty, value);
        }
        
        public double NoiseOpacity
        {
            get => (double) GetValue(NoiseOpacityProperty);
            set => SetValue(NoiseOpacityProperty, value);
        }
        
        public object TintContent
        {
            get => GetValue(TintContentProperty);
            set => SetValue(TintContentProperty, value);
        }
        
        public Brush TintBrush
        {
            get => (Brush) GetValue(TintBrushProperty);
            set => SetValue(TintBrushProperty, value);
        }
        
        public string TintText
        {
            get => (string) GetValue(TintTextProperty);
            set => SetValue(TintTextProperty, value);
        }
    }
}
