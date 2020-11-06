using Otor.MsixHero.App.Mvvm;

namespace Otor.MsixHero.App.Modules.Main.Sidebar.ViewModels
{
    public class SidebarViewModel : NotifyPropertyChanged
    {
        public SidebarViewModel()
        {
        }

        public string Text { get; } = "Sidebar";
    }
}
