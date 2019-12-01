using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using otor.msixhero.lib.Infrastructure;
using otor.msixhero.ui.Modules.Dialogs.CertificateExport.ViewModel;

namespace otor.msixhero.ui.Modules.Dialogs.CertificateExport.View
{
    /// <summary>
    /// Interaction logic for CertificateExport.
    /// </summary>
    public partial class CertificateExportView
    {
        private readonly IInteractionService interactionService;

        public CertificateExportView(IInteractionService interactionService)
        {
            this.interactionService = interactionService;
            this.InitializeComponent();
        }

        private void SaveExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            ((CertificateExportViewModel)this.DataContext).Save().ContinueWith(t =>
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
            var dataContext = ((CertificateExportViewModel) this.DataContext);
            e.CanExecute = dataContext.CanCloseDialog() && dataContext.CanSave();
            e.ContinueRouting = !e.CanExecute;
        }

        private void CloseExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            // ReSharper disable once PossibleNullReferenceException
            Window.GetWindow(this).Close();
        }

        private void CommandBinding_OnExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            if (e.Command == ApplicationCommands.Open)
            {
                if (e.Parameter as TextBox == this.PathInput)
                {
                    ((CertificateExportViewModel) this.DataContext).BrowseForInput.Execute(e.Parameter);
                }
                else if (e.Parameter as TextBox == this.PathOutput)
                {
                    ((CertificateExportViewModel)this.DataContext).BrowseForOutput.Execute(e.Parameter);
                }
            }
        }

        private void CommandBinding_OnCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            if (e.Command == ApplicationCommands.Open)
            {
                if (e.Parameter as TextBox == this.PathInput)
                {
                    e.CanExecute = ((CertificateExportViewModel)this.DataContext).BrowseForInput.CanExecute(e.Parameter);
                }
                else if (e.Parameter as TextBox == this.PathOutput)
                {
                    e.CanExecute = ((CertificateExportViewModel)this.DataContext).BrowseForOutput.CanExecute(e.Parameter);
                }
            }
        }

        private void Hyperlink_OnClick(object sender, RoutedEventArgs e)
        {
            Process.Start("explorer.exe", "/select," + this.PathOutput.Text);
        }
    }
}
