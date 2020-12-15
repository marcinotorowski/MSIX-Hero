using System.Windows;

namespace Otor.MsixHero.App.Dialogs.Views
{
    /// <summary>
    /// Interaction logic for PackageExpertWindow.xaml
    /// </summary>
    public partial class PackageExpertDialogView
    {
        public PackageExpertDialogView()
        {
            InitializeComponent();
        }

        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            // ReSharper disable once PossibleNullReferenceException
            Window.GetWindow(this).Close();
        }
    }
}
