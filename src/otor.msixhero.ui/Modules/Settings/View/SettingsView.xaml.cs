using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using otor.msixhero.lib.Infrastructure;
using otor.msixhero.ui.Modules.Settings.ViewModel;
using Prism.Services.Dialogs;

namespace otor.msixhero.ui.Modules.Settings.View
{
    /// <summary>
    /// Interaction logic for SettingsView.xaml
    /// </summary>
    public partial class SettingsView
    {
        private readonly IInteractionService interactionService;

        public SettingsView(IInteractionService interactionService)
        {
            this.interactionService = interactionService;
            InitializeComponent();
        }

        private void CloseExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            // ReSharper disable once PossibleNullReferenceException
            Window.GetWindow(this).Close();
        }

        private void SaveExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            var closeWindow = e.Parameter != null && e.Parameter is bool parameter && parameter;

            ((SettingsViewModel)this.DataContext).Save().ContinueWith(t =>
                {
                    if (t.Exception == null && !t.IsCanceled && !t.IsFaulted && t.IsCompleted)
                    {
                        if (closeWindow && t.Result)
                        {
                            // ReSharper disable once PossibleNullReferenceException
                            Window.GetWindow(this).Close();
                        }
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
            var dataContext = ((SettingsViewModel)this.DataContext);
            e.CanExecute = dataContext.CanCloseDialog() && dataContext.CanSave();
            e.ContinueRouting = !e.CanExecute;
        }
    }
}
