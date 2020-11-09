using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Otor.MsixHero.App.Controls.PackageExpert.Views
{
    public class Pill : ContentControl
    {
        public static readonly DependencyProperty IconProperty = DependencyProperty.Register("Icon", typeof(Geometry), typeof(Pill), new PropertyMetadata(null));

        public Geometry Icon
        {
            get => (Geometry)GetValue(IconProperty);
            set => SetValue(IconProperty, value);
        }
    }
}
