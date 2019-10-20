using MSI_Hero.Modules.Main;
using Prism.Regions;

namespace MSI_Hero
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
