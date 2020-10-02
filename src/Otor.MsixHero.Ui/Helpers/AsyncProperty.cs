using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Otor.MsixHero.Infrastructure.Progress;
using Otor.MsixHero.Ui.ViewModel;

namespace Otor.MsixHero.Ui.Helpers
{
    public class AsyncProperty<T> : NotifyPropertyChanged
    {
        private T currentValue;
        private bool isLoading;
        private bool hasError;
        private string error;

        public AsyncProperty(T initialValue = default, bool isLoading = false)
        {
            this.isLoading = isLoading;
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
                            this.CurrentValue = (T) Activator.CreateInstance(typeof(ObservableCollection<>).MakeGenericType(typeof(T).GetGenericArguments()[0]));
                        }
                        else
                        {
                            ((IList) this.CurrentValue).Clear();
                        }

                        foreach (var item in (IList) newValue)
                        {
                            ((IList) this.CurrentValue).Add(item);
                        }

                        this.HasValue = true;
                        return;
                    }
                }

                this.HasValue = true;
                this.CurrentValue = newValue;

                this.HasError = false;
                this.Error = null;
            }
            catch (Exception e)
            {
                this.Error = e.Message;
                this.HasError = !string.IsNullOrEmpty(e.Message);
            }
            finally
            {
                this.IsLoading = false;
            }
        }

        public bool HasError
        {
            get => this.hasError;
            private set => this.SetField(ref this.hasError, value);
        }

        public string Error
        {
            get => this.error;
            private set => this.SetField(ref this.error, value);
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
