using System;
using Otor.MsixHero.App.Hero;
using Otor.MsixHero.App.Mvvm;
using Prism.Regions;

namespace Otor.MsixHero.App.Modules.Volumes.ViewModels
{
    public class VolumesViewModel : NotifyPropertyChanged
    {
        private readonly IMsixHeroApplication application;
        private bool isActive;
        private bool firstRun = true;

        public VolumesViewModel(IMsixHeroApplication application)
        {
            this.application = application;
        }

        public bool IsActive
        {
            get => this.isActive;
            set
            {
                if (!this.SetField(ref this.isActive, value))
                {
                    return;
                }

                if (value && firstRun)
                {
                    firstRun = false;
                }

                this.IsActiveChanged?.Invoke(this, new EventArgs());
            }
        }

        public event EventHandler IsActiveChanged;

        public void OnNavigatedTo(NavigationContext navigationContext)
        {
        }

        public bool IsNavigationTarget(NavigationContext navigationContext)
        {
            return true;
        }

        public void OnNavigatedFrom(NavigationContext navigationContext)
        {
        }
    }
}
