using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
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
using Otor.MsixHero.Appx.Signing.DeviceGuard;

namespace Otor.MsixHero.DeviceGuardLoginHelper
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private async void ClickMe(object sender, RoutedEventArgs e)
        {
            var dgss = new DgssTokenCreator();
            var r = await dgss.SignIn(CancellationToken.None).ConfigureAwait(true);
            this.Label.Text = "access = " + r.AccessToken + System.Environment.NewLine + "refresh = " + r.RefreshToken;
        }
    }
}
