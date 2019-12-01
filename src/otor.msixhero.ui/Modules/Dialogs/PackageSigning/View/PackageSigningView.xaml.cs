using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using otor.msixhero.lib.Infrastructure;
using otor.msixhero.ui.Modules.Dialogs.PackageSigning.ViewModel;
using Prism.Services.Dialogs;

namespace otor.msixhero.ui.Modules.Dialogs.PackageSigning.View
{
    /// <summary>
    /// Interaction logic for PackageSigningView.
    /// </summary>
    public partial class PackageSigningView
    {
        private readonly IInteractionService interactionService;

        public PackageSigningView(IInteractionService interactionService)
        {
            this.interactionService = interactionService;
            this.InitializeComponent();
        }

        private void SaveExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            ((PackageSigningViewModel)this.DataContext).Save().ContinueWith(t =>
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
            var dataContext = ((PackageSigningViewModel) this.DataContext);
            e.CanExecute = dataContext.CanCloseDialog() && dataContext.CanSave();
            e.ContinueRouting = !e.CanExecute;
        }

        private void CloseExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            // ReSharper disable once PossibleNullReferenceException
            Window.GetWindow(this).Close();
        }

        private void CanOpenExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = this.PfxFile.IsChecked == true;
        }

        private void OpenExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            if (!this.interactionService.SelectFile("PFX files (*.pfx)|*.pfx", out string selectedFile))
            {
                return;
            }

            ((PackageSigningViewModel)this.DataContext).PfxPath = selectedFile;
        }

        private void OnDrop(object sender, DragEventArgs e)
        {
            DropFileObject.SetIsDragging((DependencyObject)sender, false);

            var hasData = e.Data.GetDataPresent("FileDrop");
            if (!hasData)
            {
                return;
            }

            var data = e.Data.GetData("FileDrop") as string[];
            if (data == null || !data.Any())
            {
                return;
            }

            var files = ((PackageSigningViewModel) this.DataContext).Files;
            foreach (var item in data)
            {
                if (files.Contains(item))
                {
                    continue;
                }

                files.Add(item);
            }
        }

        private void OnDragEnter(object sender, DragEventArgs e)
        {
            DropFileObject.SetIsDragging((DependencyObject)sender, false);

            var hasData = e.Data.GetDataPresent("FileDrop");
            if (!hasData)
            {
                return;
            }

            var data = e.Data.GetData("FileDrop") as string[];
            if (data == null || !data.Any())
            {
                return;
            }

            DropFileObject.SetIsDragging((DependencyObject) sender, true);
        }

        private void OnDragLeave(object sender, DragEventArgs e)
        {
            DropFileObject.SetIsDragging((DependencyObject)sender, false);
        }

        private void CommandBinding_OnExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            if (e.Command == ApplicationCommands.Delete)
            {
                var selectedItems = this.ListBox.SelectedItems;
                var list = ((PackageSigningViewModel)this.DataContext).Files;

                for (var i = selectedItems.Count - 1; i >= 0; i--)
                {
                    var item = (string)selectedItems[i];
                    list.Remove(item);
                }
            }
            else if (e.Command == ApplicationCommands.New)
            {
                ((PackageSigningViewModel)this.DataContext).OnDialogOpened(new DialogParameters());
            }
        }

        private void CommandBinding_OnCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            if (e.Command == ApplicationCommands.Delete)
            {
                var selectedItems = this.ListBox == null ? null : this.ListBox.SelectedItems;
                // ReSharper disable once ConditionIsAlwaysTrueOrFalse
                e.CanExecute = selectedItems != null && selectedItems.Count != 0;
            }
            else if (e.Command == ApplicationCommands.New)
            {
                e.CanExecute = true;
            }
        }

        private void PasswordBox_OnPasswordChanged(object sender, RoutedEventArgs e)
        {
            ((PackageSigningViewModel)this.DataContext).Password = ((PasswordBox)sender).SecurePassword;
        }
    }
}
