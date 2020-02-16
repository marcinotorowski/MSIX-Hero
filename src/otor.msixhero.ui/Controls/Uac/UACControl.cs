using System.Windows;
using System.Windows.Controls;

namespace otor.msixhero.ui.Controls.Uac
{
    // ReSharper disable once InconsistentNaming
    public class UACControl : ContentControl
    {
        static UACControl()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(UACControl), new FrameworkPropertyMetadata(typeof(UACControl)));
        }

        public static readonly DependencyProperty ShowShieldProperty =  DependencyProperty.Register("ShowShield", typeof(bool), typeof(UACControl), new PropertyMetadata(false));

        public bool ShowShield
        {
            get => (bool)this.GetValue(ShowShieldProperty);
            set => this.SetValue(ShowShieldProperty, value);
        }
    }
}
