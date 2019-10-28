using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using otor.msixhero.ui.Modules.Dialogs.NewSelfSigned.ViewModel;

namespace otor.msixhero.ui.Modules.Dialogs.NewSelfSigned.View
{
    /// <summary>
    /// Interaction logic for NewSelfSignedView.xaml
    /// </summary>
    public partial class NewSelfSignedView : UserControl
    {
        public NewSelfSignedView()
        {
            InitializeComponent();
        }

        private void SaveExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            ((NewSelfSignedViewModel)this.DataContext).Save().GetAwaiter().GetResult();
            // ReSharper disable once PossibleNullReferenceException
            Window.GetWindow(this).Close();
        }

        private void CanSave(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = ((NewSelfSignedViewModel) this.DataContext).CanCloseDialog();
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
