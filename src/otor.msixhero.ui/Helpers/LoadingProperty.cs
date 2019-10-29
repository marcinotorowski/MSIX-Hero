using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using otor.msixhero.lib.Domain;
using otor.msixhero.ui.ViewModel;

namespace otor.msixhero.ui.Helpers
{
    public class AsyncProperty<T> : NotifyPropertyChanged
    {
        private T currentValue;
        private bool isLoading;

        public AsyncProperty()
        {
        }

        public AsyncProperty(Task<T> loader, IProgress<Progress.ProgressData> progressReporter = null)
        {
            if (loader != null)
            {
#pragma warning disable 4014
                this.Load(loader, progressReporter);
#pragma warning restore 4014
            }
        }

        public async Task Load(Task<T> loader, IProgress<Progress.ProgressData> progressReporter = null)
        {
            try
            {
                this.IsLoading = true;
                var newValue = await loader.ConfigureAwait(false);
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
            private set => this.SetField(ref this.currentValue, value);
        }

        public bool IsLoading
        {
            get => this.isLoading;
            private set => this.SetField(ref this.isLoading, value);
        }
    }
}
