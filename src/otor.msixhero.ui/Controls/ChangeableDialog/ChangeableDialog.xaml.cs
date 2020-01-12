using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using otor.msixhero.ui.Controls.ChangeableDialog.ViewModel;
using Prism.Services.Dialogs;

namespace otor.msixhero.ui.Controls.ChangeableDialog
{
    /// <summary>
    /// Interaction logic for ChangeableDialog.xaml
    /// </summary>
    public partial class ChangeableDialog
    {
        public static readonly DependencyProperty SuccessContentTemplateProperty = DependencyProperty.Register("SuccessContentTemplate", typeof(DataTemplate), typeof(ChangeableDialog), new PropertyMetadata(null));

        public static readonly DependencyProperty SuccessContentTemplateSelectorProperty = DependencyProperty.Register("SuccessContentTemplateSelector", typeof(DataTemplateSelector), typeof(ChangeableDialog), new PropertyMetadata(null));

        public static readonly DependencyProperty SuccessContentProperty = DependencyProperty.Register("SuccessContent", typeof(object), typeof(ChangeableDialog), new PropertyMetadata(null));

        public static readonly DependencyProperty DialogContentTemplateProperty = DependencyProperty.Register("DialogContentTemplate", typeof(DataTemplate), typeof(ChangeableDialog), new PropertyMetadata(null));

        public static readonly DependencyProperty DialogContentTemplateSelectorProperty = DependencyProperty.Register("DialogContentTemplateSelector", typeof(DataTemplateSelector), typeof(ChangeableDialog), new PropertyMetadata(null));

        public static readonly DependencyProperty DialogContentProperty = DependencyProperty.Register("DialogContent", typeof(object), typeof(ChangeableDialog), new PropertyMetadata(null));

        public static readonly DependencyProperty IconProperty = DependencyProperty.Register("Icon", typeof(Geometry), typeof(ChangeableDialog), new PropertyMetadata(Geometry.Empty));
        
        public static readonly DependencyProperty OkButtonLabelProperty = DependencyProperty.Register("OkButtonLabel", typeof(object), typeof(ChangeableDialog), new PropertyMetadata(null));

        public ChangeableDialog()
        {
            InitializeComponent();
        }

        public Geometry Icon
        {
            get => (Geometry)this.GetValue(IconProperty);
            set => this.SetValue(IconProperty, value);
        }
        
        public object OkButtonLabel
        {
            get => this.GetValue(OkButtonLabelProperty);
            set => this.SetValue(OkButtonLabelProperty, value);
        }
        
        public object SuccessContent
        {
            get => this.GetValue(SuccessContentProperty);
            set => this.SetValue(SuccessContentProperty, value);
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

        public object DialogContent
        {
            get => this.GetValue(DialogContentProperty);
            set => this.SetValue(DialogContentProperty, value);
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

        private void CanSave(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = !(this.DataContext is ChangeableDialogViewModel dataContext) || dataContext.CanSave();
        }

        private void SaveExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            if (this.DataContext is ChangeableDialogViewModel dataContext)
            {
                dataContext.Save(e.Parameter is bool boolParam && boolParam);
            }
        }

        private void CloseExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            if (this.DataContext is ChangeableDialogViewModel dataContext)
            {
                dataContext.Close(dataContext.State.IsSaved ? ButtonResult.OK : ButtonResult.Cancel);
            }
        }
    }
}
