using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using otor.msixhero.lib.Infrastructure;
using otor.msixhero.ui.Modules.Dialogs.AppInstaller.ViewModel;

namespace otor.msixhero.ui.Modules.Dialogs.AppInstaller.View
{
    /// <summary>
    /// Interaction logic for AppInstallerView.
    /// </summary>
    public partial class AppInstallerView
    {
        private readonly IInteractionService interactionService;

        public AppInstallerView(IInteractionService interactionService)
        {
            this.interactionService = interactionService;
            this.InitializeComponent();
        }

        private void SaveExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            ((AppInstallerViewModel)this.DataContext).Save().ContinueWith(t =>
            {
                if (t.Exception == null && !t.IsCanceled && !t.IsFaulted && t.IsCompleted)
                {
                    // ReSharper disable once PossibleNullReferenceException
                    // Window.GetWindow(this).Close();
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
            var dataContext = ((AppInstallerViewModel)this.DataContext);
            e.CanExecute = dataContext.CanCloseDialog() && dataContext.CanSave();
            e.ContinueRouting = !e.CanExecute;
        }

        private void CloseExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            // ReSharper disable once PossibleNullReferenceException
            Window.GetWindow(this).Close();
        }
    }
}
