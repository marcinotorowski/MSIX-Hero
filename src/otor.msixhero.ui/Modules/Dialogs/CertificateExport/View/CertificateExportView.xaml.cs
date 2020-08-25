using System.Diagnostics;
using System.Windows;
using Otor.MsixHero.Ui.Modules.Dialogs.CertificateExport.ViewModel;

namespace Otor.MsixHero.Ui.Modules.Dialogs.CertificateExport.View
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
            var pathOutput = ((CertificateExportViewModel)this.DataContext).OutputPath.CurrentValue;
            Process.Start("explorer.exe", "/select," + pathOutput);
        }
    }
}
