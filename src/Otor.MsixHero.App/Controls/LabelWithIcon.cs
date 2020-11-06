using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Otor.MsixHero.App.Controls
{
    public class LabelWithIcon : ContentControl
    {
        static LabelWithIcon()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(LabelWithIcon), new FrameworkPropertyMetadata(typeof(LabelWithIcon)));
        }

        public static readonly DependencyProperty Icon16x16Property = DependencyProperty.Register("Icon16x16", typeof(Geometry), typeof(LabelWithIcon), new PropertyMetadata(Geometry.Empty));

        public Geometry Icon16x16   
        {
            get => (Geometry)GetValue(Icon16x16Property);
            set => SetValue(Icon16x16Property, value);
        }
    }
}
