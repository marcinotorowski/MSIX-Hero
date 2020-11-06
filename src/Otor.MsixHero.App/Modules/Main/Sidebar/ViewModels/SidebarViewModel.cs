using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Media;
using Otor.MsixHero.App.Mvvm;
using Prism.Modularity;
using Prism.Regions;

namespace Otor.MsixHero.App.Modules.Main.Sidebar.ViewModels
{
    public class SidebarViewModel : NotifyPropertyChanged
    {
        private readonly IRegionManager regionManager;
        private readonly IModuleManager moduleManager;
        private static readonly Geometry TabPackages = Geometry.Parse("M 16 4 L 15.625 4.15625 L 9.625 6.5625 L 9 6.8125 L 9 13.53125 L 3.59375 15.875 L 3 16.15625 L 3 24.21875 L 3.5 24.53125 L 9.5 27.875 L 9.96875 28.125 L 10.4375 27.90625 L 16 25.125 L 21.5625 27.90625 L 22.03125 28.125 L 22.5 27.875 L 28.5 24.53125 L 29 24.21875 L 29 16.15625 L 28.40625 15.875 L 23 13.53125 L 23 6.8125 L 22.375 6.5625 L 16.375 4.15625 Z M 16 6.1875 L 19.28125 7.46875 L 16 8.75 L 12.71875 7.46875 Z M 11 8.9375 L 15 10.46875 L 15 15.34375 L 11 13.5625 Z M 21 8.9375 L 21 13.5625 L 17 15.34375 L 17 10.46875 Z M 10 15.3125 L 13.625 16.90625 L 10 18.6875 L 6.375 16.875 Z M 22 15.3125 L 25.625 16.875 L 22 18.6875 L 18.375 16.90625 L 19.5 16.40625 Z M 5 18.40625 L 9 20.40625 L 9 25.3125 L 5 23.0625 Z M 27 18.40625 L 27 23.0625 L 23 25.3125 L 23 20.40625 Z M 15 18.46875 L 15 23.375 L 11 25.375 L 11 20.40625 Z M 17 18.46875 L 21 20.40625 L 21 25.375 L 17 23.375 Z");

        private static readonly Geometry TabVolumes = Geometry.Parse("M 6.21875 6 L 3 18.875 L 3 26 L 29 26 L 29 18.875 L 25.78125 6 Z M 7.78125 8 L 24.21875 8 L 26.71875 18 L 5.28125 18 Z M 5 20 L 27 20 L 27 24 L 5 24 Z M 24 21 C 23.449219 21 23 21.449219 23 22 C 23 22.550781 23.449219 23 24 23 C 24.550781 23 25 22.550781 25 22 C 25 21.449219 24.550781 21 24 21 Z");

        private static readonly Geometry TabSystemStatus = Geometry.Parse("M 2 6 L 2 24 L 15 24 L 15 26 L 10 26 L 10 28 L 22 28 L 22 26 L 17 26 L 17 24 L 30 24 L 30 6 Z M 4 8 L 28 8 L 28 22 L 4 22 Z M 13.125 8.5 L 10.375 14 L 5 14 L 5 16 L 11.625 16 L 12.875 13.5 L 15.0625 19.34375 L 15.71875 21.0625 L 16.8125 19.59375 L 19.5 16 L 24 16 L 24 14 L 18.5 14 L 18.1875 14.40625 L 16.28125 16.9375 L 13.9375 10.65625 Z");

        private static readonly Geometry TabEventViewer = Geometry.Parse("M 24 0 L 21.714844 4 L 6 4 L 6 9 L 5 9 L 5 11 L 8 11 L 8 10 L 8 9 L 8 6 L 20.572266 6 L 16 14 L 24 14 L 24 16 C 19.6 16 16 19.6 16 24 C 16 24.691044 16.098874 25.35927 16.265625 26 L 8 26 L 8 22 L 6 22 L 6 28 L 17.087891 28 C 18.477015 30.384094 21.05638 32 24 32 C 28.4 32 32 28.4 32 24 C 32 20.291044 29.438915 17.160607 26 16.265625 L 26 14 L 32 14 L 24 0 z M 23 5 L 25 5 L 25.03125 9 L 23 9 L 23 5 z M 23 10 L 25 10 L 25 12 L 23 12 L 23 10 z M 6 12 L 6 14 L 5 14 L 5 16 L 8 16 L 8 14 L 8 12 L 6 12 z M 6 17 L 6 19 L 5 19 L 5 21 L 8 21 L 8 19 L 8 17 L 6 17 z M 24 18 C 27.3 18 30 20.7 30 24 C 30 27.3 27.3 30 24 30 C 20.7 30 18 27.3 18 24 C 18 20.7 20.7 18 24 18 z M 21.699219 20.300781 L 20.300781 21.699219 L 22.599609 24 L 20.300781 26.300781 L 21.699219 27.699219 L 24 25.400391 L 26.300781 27.699219 L 27.699219 26.300781 L 25.400391 24 L 27.699219 21.699219 L 26.300781 20.300781 L 24 22.599609 L 21.699219 20.300781 z");

        private SidebarItemViewModel selectedItem;

        public SidebarViewModel(IRegionManager regionManager, IModuleManager moduleManager)
        {
            this.regionManager = regionManager;
            this.moduleManager = moduleManager;
            this.SidebarItems = new ObservableCollection<SidebarItemViewModel>
            {
                new SidebarItemViewModel(
                    ModuleNames.Packages,
                    PathNames.Packages,
                    "Packages",
                    TabPackages),

                new SidebarItemViewModel(
                    ModuleNames.Volumes,
                    PathNames.Volumes,
                    "Volumes",
                    TabVolumes),

                new SidebarItemViewModel(
                    ModuleNames.EventViewer,
                    PathNames.EventViewer,
                    "Event viewer",
                    TabEventViewer),

                new SidebarItemViewModel(
                    ModuleNames.SystemView, 
                    PathNames.SystemView,
                    "System overview",
                    TabSystemStatus)
            };

            this.SelectedItem = this.SidebarItems.First();
        }

        public ObservableCollection<SidebarItemViewModel> SidebarItems { get; }

        public SidebarItemViewModel SelectedItem
        {
            get => this.selectedItem;
            set
            {
                if (!this.SetField(ref this.selectedItem, value))
                {
                    return;
                }

                if (value != null)
                {
                    this.moduleManager.LoadModule(value.ModuleId);
                    this.regionManager.Regions[RegionNames.Main].RequestNavigate(value.Name);
                }
            }
        }
    }
}
