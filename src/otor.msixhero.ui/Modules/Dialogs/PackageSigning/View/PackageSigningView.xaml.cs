using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Otor.MsixHero.Ui.Modules.Dialogs.PackageSigning.ViewModel;
using Prism.Services.Dialogs;

namespace Otor.MsixHero.Ui.Modules.Dialogs.PackageSigning.View
{
    /// <summary>
    /// Interaction logic for PackageSigningView.
    /// </summary>
    public partial class PackageSigningView
    {
        public PackageSigningView()
        {
            this.InitializeComponent();
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
                var selectedItems = ((PackageSigningViewModel)this.DataContext).SelectedPackages;
                var list = ((PackageSigningViewModel)this.DataContext).Files;

                for (var i = selectedItems.Count - 1; i >= 0; i--)
                {
                    var item = selectedItems[i];
                    list.Remove(item);
                }
            }
            else if (e.Command == ApplicationCommands.New)
            {
                ((IDialogAware)this.DataContext).OnDialogOpened(new DialogParameters());
            }
        }

        private void CommandBinding_OnCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            if (e.Command == ApplicationCommands.Delete)
            {
                var selectedItems = ((PackageSigningViewModel)this.DataContext).SelectedPackages;
                // ReSharper disable once ConditionIsAlwaysTrueOrFalse
                e.CanExecute = selectedItems != null && selectedItems.Count != 0;
            }
            else if (e.Command == ApplicationCommands.New)
            {
                e.CanExecute = true;
            }
        }

        private void OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var sp = ((PackageSigningViewModel) this.DataContext).SelectedPackages;
            sp.Clear();
            sp.AddRange(((ListBox)sender).SelectedItems.OfType<string>());
        }
    }
}
