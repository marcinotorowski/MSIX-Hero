using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using otor.msixhero.lib.Infrastructure;
using otor.msixhero.ui.Modules.Dialogs.NewSelfSigned.ViewModel;

namespace otor.msixhero.ui.Modules.Dialogs.NewSelfSigned.View
{
    /// <summary>
    /// Interaction logic for NewSelfSignedView.
    /// </summary>
    public partial class NewSelfSignedView
    {
        private readonly IInteractionService interactionService;

        public NewSelfSignedView(IInteractionService interactionService)
        {
            this.interactionService = interactionService;
            this.InitializeComponent();
        }

        private void SaveExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            ((NewSelfSignedViewModel)this.DataContext).Save().ContinueWith(t =>
            {
                if (t.Exception == null && !t.IsCanceled && !t.IsFaulted && t.IsCompleted)
                {
                    // ReSharper disable once PossibleNullReferenceException
                    Window.GetWindow(this).Close();
                }
                else if (t.IsFaulted && t.Exception != null)
                {
                    var exception = t.Exception.GetBaseException();
                    var result = this.interactionService.ShowError(exception.Message, extendedInfo: exception.ToString());
                    if (result == InteractionResult.Retry)
                    {
                        this.SaveExecuted(sender, e);
                    }
                }
            },
            CancellationToken.None, 
            TaskContinuationOptions.ExecuteSynchronously | TaskContinuationOptions.AttachedToParent, 
            TaskScheduler.FromCurrentSynchronizationContext());
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
