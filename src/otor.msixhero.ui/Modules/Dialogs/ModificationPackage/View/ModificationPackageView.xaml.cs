using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using otor.msixhero.lib.Infrastructure;
using otor.msixhero.ui.Modules.Dialogs.ModificationPackage.ViewModel;

namespace otor.msixhero.ui.Modules.Dialogs.ModificationPackage.View
{
    /// <summary>
    /// Interaction logic for ModificationPackageView.
    /// </summary>
    public partial class ModificationPackageView
    {
        private readonly IInteractionService interactionService;

        public ModificationPackageView(IInteractionService interactionService)
        {
            this.interactionService = interactionService;
            this.InitializeComponent();
        }

        private void SaveExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            ((ModificationPackageViewModel)this.DataContext).Save().ContinueWith(t =>
            {
                if (t.Exception == null && !t.IsCanceled && !t.IsFaulted && t.IsCompleted)
                {
                    // ReSharper disable once PossibleNullReferenceException
                    // Window.GetWindow(this).Close();
                }
                else if (t.IsFaulted && t.Exception != null)
                {
                    var exception = t.Exception.GetBaseException();
                    var result = this.interactionService.ShowError(exception.Message, exception);
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
            var dataContext = ((ModificationPackageViewModel) this.DataContext);
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
