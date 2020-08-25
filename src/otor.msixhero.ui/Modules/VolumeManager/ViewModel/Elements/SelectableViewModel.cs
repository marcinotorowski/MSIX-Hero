using Otor.MsixHero.Ui.ViewModel;

namespace Otor.MsixHero.Ui.Modules.VolumeManager.ViewModel.Elements
{
    public abstract class SelectableViewModel<T> : NotifyPropertyChanged
    {
        private bool isSelected;

        protected SelectableViewModel(T model, bool isSelected = false)
        {
            this.Model = model;
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