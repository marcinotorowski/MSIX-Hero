using System.Diagnostics;
using System.Windows;
using Otor.MsixHero.App.Modules.Dialogs.Signing.CertificateExport.ViewModel;

namespace Otor.MsixHero.App.Modules.Dialogs.Signing.CertificateExport.View
{
    /// <summary>
    /// Interaction logic for CertificateExport.
    /// </summary>
    public partial class CertificateExportView
    {
        public CertificateExportView()
        {
            this.InitializeComponent();
        }
        
        private void Hyperlink_OnClick(object sender, RoutedEventArgs e)
        {
            var pathOutput = ((CertificateExportViewModel)this.DataContext).ExtractCertificate.CurrentValue;
            Process.Start("explorer.exe", "/select," + pathOutput);
        }
    }
}
