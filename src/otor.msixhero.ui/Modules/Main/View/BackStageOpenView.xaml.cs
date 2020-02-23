using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Fluent;
using Xceed.Wpf.Toolkit.Core.Utilities;

namespace otor.msixhero.ui.Modules.Main.View
{
    /// <summary>
    /// Interaction logic for BackStageOpenView.xaml
    /// </summary>
    public partial class BackStageOpenView : UserControl
    {
        public BackStageOpenView()
        {
            InitializeComponent();
        }

        private void BackstageButtonClicked(object sender, RoutedEventArgs e)
        {
            PopupService.RaiseDismissPopupEvent(sender, DismissPopupMode.Always);
        }
    }
}
