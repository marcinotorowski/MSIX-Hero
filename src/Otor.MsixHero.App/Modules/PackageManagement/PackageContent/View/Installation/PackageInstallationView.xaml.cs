using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Documents;
using Otor.MsixHero.App.Services;
using Otor.MsixHero.Appx.Common;
using Otor.MsixHero.Infrastructure.Helpers;

namespace Otor.MsixHero.App.Modules.PackageManagement.PackageContent.View.Installation
{
    public partial class PackageInstallationView
    {
        public PackageInstallationView()
        {
            InitializeComponent();
        }

        private void Hyperlink_OnClick(object sender, RoutedEventArgs e)
        {
            ExceptionGuard.Guard(() =>
                {
                    var dir = (string)((Hyperlink)sender).Tag;
                    Process.Start("explorer.exe", "/select," + Path.Combine(dir, AppxFileConstants.AppxManifestFile));
                },
                new InteractionService());
        }
    }
}
