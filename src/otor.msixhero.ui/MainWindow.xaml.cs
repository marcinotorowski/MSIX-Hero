using otor.msixhero.ui.Modules.Main;
using otor.msixhero.ui.Modules.Main.View;
using otor.msixhero.ui.Modules.Settings;
using Prism.Regions;

namespace otor.msixhero.ui
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        public MainWindow()
        {
            InitializeComponent();

        }

        public MainWindow(IRegionManager regionManager) : this()
        {
            regionManager.RegisterViewWithRegion("ShellRegion", typeof(MainView));
            regionManager.RequestNavigate("ShellRegion", MainModule.Path);
        }
    }
}
