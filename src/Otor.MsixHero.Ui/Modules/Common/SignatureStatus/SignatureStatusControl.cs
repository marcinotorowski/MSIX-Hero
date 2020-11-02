using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Otor.MsixHero.Ui.Modules.Common.SignatureStatus
{
    public class SignatureStatusControl : Control
    {
        public static readonly DependencyProperty IsTrustedProperty = DependencyProperty.Register("IsTrusted", typeof(bool), typeof(SignatureStatusControl), new PropertyMetadata(false));
        public static readonly DependencyProperty IsChainLoadedProperty = DependencyProperty.Register("IsChainLoaded", typeof(bool), typeof(SignatureStatusControl), new PropertyMetadata(false));
        public static readonly DependencyProperty TrusteeProperty = DependencyProperty.Register("Trustee", typeof(string), typeof(SignatureStatusControl), new PropertyMetadata(null));
        public static readonly DependencyProperty TrustMeCommandProperty = DependencyProperty.Register("TrustMeCommand", typeof(ICommand), typeof(SignatureStatusControl), new PropertyMetadata(null));
        public static readonly DependencyProperty ViewPropertiesCommandProperty = DependencyProperty.Register("ViewPropertiesCommand", typeof(ICommand), typeof(SignatureStatusControl), new PropertyMetadata(null));

        static SignatureStatusControl()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(SignatureStatusControl), new FrameworkPropertyMetadata(typeof(SignatureStatusControl)));
        }

        public ICommand TrustMeCommand
        {
            get => (ICommand)GetValue(TrustMeCommandProperty);
            set => SetValue(TrustMeCommandProperty, value);
        }

        public ICommand ViewPropertiesCommand
        {
            get => (ICommand)this.GetValue(ViewPropertiesCommandProperty);
            set => this.SetValue(ViewPropertiesCommandProperty, value);
        }

        public string Trustee
        {
            get => (string)this.GetValue(TrusteeProperty);
            set => this.SetValue(TrusteeProperty, value);
        }

        public bool IsTrusted
        {
            get => (bool)this.GetValue(IsTrustedProperty);
            set => this.SetValue(IsTrustedProperty, value);
        }

        public bool IsChainLoaded
        {
            get => (bool)this.GetValue(IsChainLoadedProperty);
            set => this.SetValue(IsChainLoadedProperty, value);
        }
    }
}
