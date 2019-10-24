using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace MSI_Hero.Modules.PackageList.View
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
