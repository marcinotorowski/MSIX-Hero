using System.Windows.Controls;
using System.Windows.Input;
using Otor.MsixHero.App.Modules.PackageManagement.Search.ViewModels;

namespace Otor.MsixHero.App.Modules.PackageManagement.Search.Views
{
    /// <summary>
    /// Interaction logic for PackageFilterSortView.xaml
    /// </summary>
    public partial class PackageFilterSortView
    {
        public PackageFilterSortView()
        {
            InitializeComponent();
        }

        private void UIElement_OnPreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            var senderRadio = (RadioButton)sender;
            if (senderRadio.IsChecked != true)
            {
                return;
            }

            if (e.Source as RadioButton != senderRadio)
            {
                return;
            }

            var dataContext = (PackageFilterSortViewModel) senderRadio.DataContext;
            dataContext.IsDescending = !dataContext.IsDescending;
        }
    }
}
