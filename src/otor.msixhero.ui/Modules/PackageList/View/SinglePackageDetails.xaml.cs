using System;
using System.Windows.Controls;
using System.Windows.Input;

namespace otor.msixhero.ui.Modules.PackageList.View
{
    /// <summary>
    /// Interaction logic for SinglePackageDetails.xaml
    /// </summary>
    public partial class SinglePackageDetails : UserControl
    {
        public SinglePackageDetails()
        {
            InitializeComponent();
        }

        private void CommandBinding_OnExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            
        }

        private void CommandBinding_OnCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            if (DateTime.Now.Second % 2 == 0)
            {
                e.CanExecute = true;
            }
            else
            {
                e.CanExecute = false;
            }
        }
    }
}
