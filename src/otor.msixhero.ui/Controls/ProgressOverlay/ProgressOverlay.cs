using System.Windows;
using System.Windows.Controls;

namespace otor.msixhero.ui.Controls.ProgressOverlay
{
    public class ProgressOverlay : ContentControl
    {
        static ProgressOverlay()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(ProgressOverlay), new FrameworkPropertyMetadata(typeof(ProgressOverlay)));
        }

        public static readonly DependencyProperty ProgressProperty =  DependencyProperty.Register("Progress", typeof(double), typeof(ProgressOverlay), new PropertyMetadata(0.0));

        public static readonly DependencyProperty MessageProperty =  DependencyProperty.Register("Message", typeof(string), typeof(ProgressOverlay), new PropertyMetadata("Please wait..."));

        public static readonly DependencyProperty IsShownProperty =  DependencyProperty.Register("IsShown", typeof(bool), typeof(ProgressOverlay), new PropertyMetadata(false));
        
        public double Progress
        {
            get => (double)this.GetValue(ProgressProperty);
            set => this.SetValue(ProgressProperty, value);
        }
        
        public string Message
        {
            get => (string)this.GetValue(MessageProperty);
            set => this.SetValue(MessageProperty, value);
        }


        public bool IsShown
        {
            get => (bool)this.GetValue(IsShownProperty);
            set => this.SetValue(IsShownProperty, value);
        }
    }
}
