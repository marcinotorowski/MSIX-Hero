using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Otor.MsixHero.Ui.Modules.Dialogs.PackageExpert.ViewModel;

namespace Otor.MsixHero.Ui.Modules.Dialogs.PackageExpert.View
{
    /// <summary>
    /// Interaction logic for PackageExpertView.xaml
    /// </summary>
    public partial class PackageExpertView : UserControl
    {
        public PackageExpertView()
        {
            InitializeComponent();
            this.KeyUp+= OnKeyUp;
        }

        private void OnKeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Space)
            {
                var dc = ((PackageExpertViewModel) this.DataContext);
                this.PackageContentView.Content = dc.Content;
            }
        }

        private void OnCloseExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            Window.GetWindow(this).Close();
        }
    }
}
