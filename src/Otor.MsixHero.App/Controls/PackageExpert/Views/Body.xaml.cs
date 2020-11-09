using System;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Documents;

namespace Otor.MsixHero.App.Controls.PackageExpert.Views
{
    /// <summary>
    /// Interaction logic for PackageExpert
    /// </summary>
    public partial class Body
    {
        public Body()
        {
            this.InitializeComponent();
        }

        private void HyperlinkOnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                var psi = new ProcessStartInfo((string)((Hyperlink)sender).Tag)
                {
                    UseShellExecute = true
                };

                Process.Start(psi);
            }
            catch (Exception exception)
            {
                // Logger.Error(exception);
            }
        }

        private void Hyperlink_OnClick(object sender, RoutedEventArgs e)
        {
            var dir = (string)((Hyperlink)sender).Tag;
            Process.Start("explorer.exe", "/select," + Path.Combine(dir, "AppxManifest.xml"));
        }
    }
}
