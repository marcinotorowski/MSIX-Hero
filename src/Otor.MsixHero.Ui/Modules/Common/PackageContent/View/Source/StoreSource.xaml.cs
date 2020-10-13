using System.Diagnostics;
using System.Windows;

namespace Otor.MsixHero.Ui.Modules.Common.PackageContent.View.Source
{
    /// <summary>
    /// Interaction logic for StoreSource.xaml
    /// </summary>
    public partial class StoreSource
    {
        public static readonly DependencyProperty FamilyNameProperty = DependencyProperty.Register("FamilyName", typeof(string), typeof(StoreSource), new PropertyMetadata(null));

        public StoreSource()
        {
            InitializeComponent();
        }

        public string FamilyName
        {
            get => (string)GetValue(FamilyNameProperty);
            set => SetValue(FamilyNameProperty, value);
        }

        private void OpenStorePage(object sender, RoutedEventArgs e)
        {
            var link = $"ms-windows-store://pdp/?PFN={this.FamilyName}";
            var psi = new ProcessStartInfo(link);
            psi.UseShellExecute = true;
            Process.Start(psi);
        }

        private void WriteReview(object sender, RoutedEventArgs e)
        {
            var link = $"ms-windows-store://review/?PFN={this.FamilyName}";
            var psi = new ProcessStartInfo(link);
            psi.UseShellExecute = true;
            Process.Start(psi);
        }
    }
}
