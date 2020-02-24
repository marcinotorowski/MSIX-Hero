using otor.msixhero.lib.BusinessLayer.State;
using otor.msixhero.ui.ViewModel;

namespace otor.msixhero.ui.Modules.VolumeManager.ViewModel.Elements
{
    public abstract class SelectableViewModel<T> : NotifyPropertyChanged
    {
        protected readonly IApplicationStateManager StateManager;
        private bool isSelected;

        protected SelectableViewModel(T model, IApplicationStateManager stateManager, bool isSelected = false)
        {
            this.Model = model;
            this.StateManager = stateManager;
            this.isSelected = isSelected;
        }

        public T Model { get; }

        public bool IsSelected
        {
            get => this.isSelected;
            set
            {
                if (!this.SetField(ref this.isSelected, value))
                {
                    return;
                }

                if (value)
                {
                    this.TrySelect();
                }
                else
                {
                    this.TryUnselect();
                }
            }
        }

        protected abstract bool TrySelect();
        
        protected abstract bool TryUnselect();
    }
}