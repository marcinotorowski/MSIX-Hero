using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Otor.MsixHero.App.Modules.Dialogs.Signing.PackageSigning.ViewModel;

namespace Otor.MsixHero.App.Modules.Dialogs.Signing.PackageSigning.View
{
    /// <summary>
    /// Interaction logic for PackageSigningDialogContent.
    /// </summary>
    public partial class PackageSigningDialogContent
    {
        public PackageSigningDialogContent()
        {
            InitializeComponent();
        }

        private void OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var sp = ((PackageSigningViewModel)this.DataContext).SelectedPackages;
            sp.Clear();
            sp.AddRange(((ListBox)sender).SelectedItems.OfType<string>());
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

            var files = ((PackageSigningViewModel)this.DataContext).Files;
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

            DropFileObject.SetIsDragging((DependencyObject)sender, true);
        }

        private void OnDragLeave(object sender, DragEventArgs e)
        {
            DropFileObject.SetIsDragging((DependencyObject)sender, false);
        }
    }
}
