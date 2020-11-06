using System.Windows.Media;
using Otor.MsixHero.App.Mvvm;

namespace Otor.MsixHero.App.Modules.Main.Sidebar.ViewModels
{
    public class SidebarItemViewModel : NotifyPropertyChanged
    {
        private bool isChecked;

        public SidebarItemViewModel(string moduleId, string name, string title, Geometry icon)
        {
            this.ModuleId = moduleId;
            this.Icon = icon;
            this.Name = name;
            this.Title = title;
        }

        public string ModuleId { get; }

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
