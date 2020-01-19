using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using otor.msixhero.lib.Infrastructure.Progress;
using otor.msixhero.ui.ViewModel;

namespace otor.msixhero.ui.Helpers
{
    public class AsyncProperty<T> : NotifyPropertyChanged
    {
        private T currentValue;
        private bool isLoading;

        public AsyncProperty(T initialValue = default)
        {
            this.currentValue = initialValue;
        }

        public AsyncProperty(Task<T> loader, IProgress<ProgressData> progressReporter = null)
        {
            if (loader != null)
            {
#pragma warning disable 4014
                this.Load(loader, progressReporter);
#pragma warning restore 4014
            }
        }

        public async Task Load(Task<T> loader, IProgress<ProgressData> progressReporter = null)
        {
            try
            {
                this.IsLoading = true;
                var newValue = await loader.ConfigureAwait(true);

                if (this.CurrentValue != null && typeof(T).IsGenericType)
                {
                    var generic = typeof(T).GetGenericTypeDefinition();
                    
                    if (typeof(ObservableCollection<>) == generic)
                    {
                        // this is an observable collection, use events
                        if (this.CurrentValue == null)
                        {
                            this.CurrentValue = (T)Activator.CreateInstance(typeof(ObservableCollection<>).MakeGenericType(typeof(T).GetGenericArguments()[0]));
                        }
                        else
                        {
                            ((IList)this.CurrentValue).Clear();
                        }

                        foreach (var item in (IList)newValue)
                        {
                            ((IList) this.CurrentValue).Add(item);
                        }

                        this.HasValue = true;
                        return;
                    }
                }

                this.HasValue = true;
                this.CurrentValue = newValue;
            }
            finally
            {
                this.IsLoading = false;
            }
        }

        public T CurrentValue
        {
            get => this.currentValue;
            private set
            {
                this.SetField(ref this.currentValue, value);
                this.OnPropertyChanged(nameof(HasValue));
            }
        }

        public bool HasValue { get; private set; }

        public bool IsLoading
        {
            get => this.isLoading;
            private set => this.SetField(ref this.isLoading, value);
        }
    }
}
