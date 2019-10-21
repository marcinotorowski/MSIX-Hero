using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace MSI_Hero.ViewModel
{
    public abstract class NotifyPropertyChanged : INotifyPropertyChanged
    {
        // boiler-plate
        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        
        protected bool SetField<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value))
            {
                return false;
            }

            field = value;
            this.OnPropertyChanged(propertyName);

            return true;
        }
    }
}