using otor.msihero.lib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using MSI_Hero.ViewModel;

namespace MSI_Hero
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow 
    {
        public MainWindow()
        {
            InitializeComponent();
        }
        private void OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var viewModel = ((ListView) sender).DataContext as PackageListViewModel;
            if (viewModel == null)
            {
                return;
            }

            if (e.AddedItems != null)
            {
                foreach (var newItem in e.AddedItems.OfType<PackageViewModel>())
                {
                    viewModel.SelectedPackages.Add(newItem);
                }
            }

            if (e.RemovedItems != null)
            {
                foreach (var oldItem in e.RemovedItems.OfType<PackageViewModel>())
                {
                    viewModel.SelectedPackages.Remove(oldItem);
                }
            }

            if (this.TabMaintenance.IsSelected || this.TabDeveloper.IsSelected)
            {
                return;
            }

            if (viewModel.SelectedPackages.Any())
            {
                this.TabMaintenance.IsSelected = true;
            }
        }
    }
}
