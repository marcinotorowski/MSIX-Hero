using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using otor.msixhero.ui.Controls.ChangeableDialog.ViewModel;

namespace otor.msixhero.ui.Controls.ChangeableDialog
{
    public class ChangeableDialog : Control
    {
        static ChangeableDialog()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(ChangeableDialog),  new FrameworkPropertyMetadata(typeof(ChangeableDialog)));
        }

        public static readonly DependencyProperty OkButtonVisibilityProperty =  DependencyProperty.Register("OkButtonVisibility", typeof(Visibility), typeof(ChangeableDialog), new PropertyMetadata(System.Windows.Visibility.Visible));
        
        public static readonly DependencyProperty SuccessContentTemplateProperty = DependencyProperty.Register("SuccessContentTemplate", typeof(DataTemplate), typeof(ChangeableDialog), new PropertyMetadata(null));

        public static readonly DependencyProperty SuccessContentTemplateSelectorProperty = DependencyProperty.Register("SuccessContentTemplateSelector", typeof(DataTemplateSelector), typeof(ChangeableDialog), new PropertyMetadata(null));

        public static readonly DependencyProperty SuccessContentProperty = DependencyProperty.Register("SuccessContent", typeof(object), typeof(ChangeableDialog), new PropertyMetadata(null));

        public static readonly DependencyProperty DialogContentTemplateProperty = DependencyProperty.Register("DialogContentTemplate", typeof(DataTemplate), typeof(ChangeableDialog), new PropertyMetadata(null));

        public static readonly DependencyProperty DialogContentTemplateSelectorProperty = DependencyProperty.Register("DialogContentTemplateSelector", typeof(DataTemplateSelector), typeof(ChangeableDialog), new PropertyMetadata(null));

        public static readonly DependencyProperty DialogContentProperty = DependencyProperty.Register("DialogContent", typeof(object), typeof(ChangeableDialog), new PropertyMetadata(null));

        public static readonly DependencyProperty IconProperty = DependencyProperty.Register("Icon", typeof(Geometry), typeof(ChangeableDialog), new PropertyMetadata(Geometry.Empty));

        public static readonly DependencyProperty OkButtonLabelProperty = DependencyProperty.Register("OkButtonLabel", typeof(object), typeof(ChangeableDialog), new PropertyMetadata(null));

        public static readonly DependencyProperty DialogProperty = DependencyProperty.Register("Dialog", typeof(ChangeableDialogViewModel), typeof(ChangeableDialog), new PropertyMetadata(null));
        
        public static readonly DependencyProperty FooterProperty =  DependencyProperty.Register("Footer", typeof(object), typeof(ChangeableDialog), new PropertyMetadata(null));

        public static readonly DependencyProperty ShowShieldProperty = DependencyProperty.Register("ShowShield", typeof(bool), typeof(ChangeableDialog), new PropertyMetadata(false));
        
        public static readonly DependencyProperty ShowHeaderProperty = DependencyProperty.Register("ShowHeader", typeof(bool), typeof(ChangeableDialog), new PropertyMetadata(true));
        
        public ChangeableDialogViewModel Dialog
        {
            get => (ChangeableDialogViewModel)this.GetValue(DialogProperty);
            set => this.SetValue(DialogProperty, value);
        }
        
        public bool ShowShield
        {
            get => (bool)this.GetValue(ShowShieldProperty);
            set => this.SetValue(ShowShieldProperty, value);
        }

        public object Footer
        {
            get => this.GetValue(FooterProperty);
            set => this.SetValue(FooterProperty, value);
        }

        public Visibility OkButtonVisibility
        {
            get => (Visibility)this.GetValue(OkButtonVisibilityProperty);
            set => this.SetValue(OkButtonVisibilityProperty, value);
        }

        public Geometry Icon
        {
            get => (Geometry)this.GetValue(IconProperty);
            set => this.SetValue(IconProperty, value);
        }

        public bool ShowHeader
        {
            get => (bool)this.GetValue(ShowHeaderProperty);
            set => this.SetValue(ShowHeaderProperty, value);
        }

        public object OkButtonLabel
        {
            get => this.GetValue(OkButtonLabelProperty);
            set => this.SetValue(OkButtonLabelProperty, value);
        }

        public DataTemplate SuccessContentTemplate
        {
            get => (DataTemplate)this.GetValue(SuccessContentTemplateProperty);
            set => this.SetValue(SuccessContentTemplateProperty, value);
        }

        public DataTemplateSelector SuccessContentTemplateSelector
        {
            get => (DataTemplateSelector)this.GetValue(SuccessContentTemplateSelectorProperty);
            set => this.SetValue(SuccessContentTemplateSelectorProperty, value);
        }

        public DataTemplate DialogContentTemplate
        {
            get => (DataTemplate)this.GetValue(DialogContentTemplateProperty);
            set => this.SetValue(DialogContentTemplateProperty, value);
        }

        public DataTemplateSelector DialogContentTemplateSelector
        {
            get => (DataTemplateSelector)this.GetValue(DialogContentTemplateSelectorProperty);
            set => this.SetValue(DialogContentTemplateSelectorProperty, value);
        }
    }
}