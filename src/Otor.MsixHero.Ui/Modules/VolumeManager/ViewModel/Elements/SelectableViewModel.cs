using Otor.MsixHero.Ui.ViewModel;

namespace Otor.MsixHero.Ui.Modules.VolumeManager.ViewModel.Elements
{
    public abstract class SelectableViewModel<T> : NotifyPropertyChanged
    {
        protected SelectableViewModel(T model)
        {
            this.Model = model;
        }

        public T Model { get; }
    }
}