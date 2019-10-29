using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using otor.msixhero.ui.Modules.Dialogs.NewSelfSigned.ViewModel;

namespace otor.msixhero.ui.Modules.Dialogs.NewSelfSigned.View
{
    /// <summary>
    /// Interaction logic for NewSelfSignedView.
    /// </summary>
    public partial class NewSelfSignedView
    {
        public NewSelfSignedView()
        {
            this.InitializeComponent();
        }

        private void SaveExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            ((NewSelfSignedViewModel)this.DataContext).Save().ContinueWith(t =>
            {
                if (t.Exception == null && !t.IsCanceled && t.IsCompleted)
                {
                    // ReSharper disable once PossibleNullReferenceException
                    Window.GetWindow(this).Close();
                }
            });
        }

        private void CanSave(object sender, CanExecuteRoutedEventArgs e)
        {
            var dataContext = ((NewSelfSignedViewModel) this.DataContext);
            e.CanExecute = dataContext.CanCloseDialog() && dataContext.CanSave();
            e.ContinueRouting = !e.CanExecute;
        }

        private void CloseExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            // ReSharper disable once PossibleNullReferenceException
            Window.GetWindow(this).Close();
        }

        private void OpenExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            var dlg = new Ookii.Dialogs.Wpf.VistaFolderBrowserDialog();
            if (dlg.ShowDialog() == true)
            {
                ((NewSelfSignedViewModel) this.DataContext).OutputPath = dlg.SelectedPath;
            }
        }

        private void OnPasswordChanged(object sender, RoutedEventArgs e)
        {
            ((NewSelfSignedViewModel) this.DataContext).Password = ((PasswordBox)sender).Password;
        }
    }
}
