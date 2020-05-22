using System.Windows;
using System.Windows.Controls;

namespace otor.msixhero.ui.Controls.CheckedContent
{
    public class CheckedContent : ContentControl
    {
        public static readonly DependencyProperty ShowGlyphProperty = DependencyProperty.Register("ShowGlyph", typeof(bool), typeof(CheckedContent), new PropertyMetadata(true));

        public static readonly DependencyProperty ShowShieldProperty = DependencyProperty.Register("ShowShield", typeof(bool), typeof(CheckedContent), new PropertyMetadata(false));

        public static readonly DependencyProperty IsCheckedProperty = DependencyProperty.Register("IsChecked", typeof(bool), typeof(CheckedContent), new PropertyMetadata(false));

        static CheckedContent()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(CheckedContent), new FrameworkPropertyMetadata(typeof(CheckedContent)));
        }

        public bool ShowGlyph
        {
            get => (bool)this.GetValue(ShowGlyphProperty);
            set => this.SetValue(ShowGlyphProperty, value);
        }


        public bool IsChecked
        {
            get => (bool)this.GetValue(IsCheckedProperty);
            set => this.SetValue(IsCheckedProperty, value);
        }

        public bool ShowShield
        {
            get => (bool)this.GetValue(ShowShieldProperty);
            set => this.SetValue(ShowShieldProperty, value);
        }
    }
}
