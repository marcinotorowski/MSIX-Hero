using System.Windows.Media;
using Otor.MsixHero.App.Hero.State;
using Otor.MsixHero.App.Mvvm;

namespace Otor.MsixHero.App.Modules.Main.Sidebar.ViewModels
{
    public class SidebarItemViewModel : NotifyPropertyChanged
    {
        private bool isChecked;

        public SidebarItemViewModel(ApplicationMode screen, string name, string title, Geometry icon)
        {
            this.Screen = screen;
            this.Icon = icon;
            this.Name = name;
            this.Title = title;
            this.IsVisible = true;
        }

        public SidebarItemViewModel(ApplicationMode screen, string name)
        {
            this.Screen = screen;
            this.Name = name;
            this.IsVisible = false;
        }

        public bool IsVisible { get; }

        public ApplicationMode Screen { get; }

        public string Name { get; }

        public string Title { get; }

        public Geometry Icon { get; }

        public bool IsChecked
        {
            get => this.isChecked;
            set => this.SetField(ref this.isChecked, value);
        }
    }
}
