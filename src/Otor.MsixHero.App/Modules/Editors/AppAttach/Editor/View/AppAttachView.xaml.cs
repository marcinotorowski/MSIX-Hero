using System.Diagnostics;
using System.Windows;
using Otor.MsixHero.App.Modules.Editors.AppAttach.Editor.ViewModel;

namespace Otor.MsixHero.App.Modules.Editors.AppAttach.Editor.View
{
    /// <summary>
    /// Interaction logic for App Attach View.
    /// </summary>
    public partial class AppAttachView
    {
        public AppAttachView()
        {
            this.InitializeComponent();
        }
        
        private void Hyperlink_OnClick(object sender, RoutedEventArgs e)
        {
            var pathOutput = ((AppAttachViewModel)this.DataContext).OutputPath;
            Process.Start("explorer.exe", "/select," + pathOutput);
        }
    }
}
