using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Otor.MsixHero.Ui.Controls.ProgressOverlay
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

        public static readonly DependencyProperty CancelCommandProperty = DependencyProperty.Register("CancelCommand", typeof(ICommand), typeof(ProgressOverlay), new PropertyMetadata(null));

        public static readonly DependencyProperty SupportsCancellingProperty =  DependencyProperty.Register("SupportsCancelling", typeof(bool), typeof(ProgressOverlay), new PropertyMetadata(false));
        
        public ICommand CancelCommand
        {
            get => (ICommand)GetValue(CancelCommandProperty);
            set => SetValue(CancelCommandProperty, value);
        }

        public bool SupportsCancelling
        {
            get => (bool)GetValue(SupportsCancellingProperty);
            set => SetValue(SupportsCancellingProperty, value);
        }

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
