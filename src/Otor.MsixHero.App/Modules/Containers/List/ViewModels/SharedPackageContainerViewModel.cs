using Otor.MsixHero.App.Mvvm;
using Otor.MsixHero.Appx.Packaging.SharedPackageContainer.Entities;

namespace Otor.MsixHero.App.Modules.Containers.List.ViewModels
{
    public class SharedPackageContainerViewModel : NotifyPropertyChanged
    {
        private readonly SharedPackageContainer _model;

        public SharedPackageContainerViewModel(SharedPackageContainer model)
        {
            this._model = model;
        }

        public SharedPackageContainer Model => this._model;

        public string Name => this._model.Name;
    }
}
