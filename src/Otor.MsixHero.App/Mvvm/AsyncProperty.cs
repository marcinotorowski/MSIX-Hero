// MSIX Hero
// Copyright (C) 2022 Marcin Otorowski
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// Full notice:
// https://github.com/marcinotorowski/msix-hero/blob/develop/LICENSE.md

using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading.Tasks;
using Otor.MsixHero.Infrastructure.Progress;

namespace Otor.MsixHero.App.Mvvm
{
    public class AsyncProperty<T> : NotifyPropertyChanged
    {
        private T currentValue;

        public AsyncProperty(T initialValue = default, bool isLoading = false)
        {
            this.Progress.IsLoading = isLoading;
            currentValue = initialValue;
        }

        public AsyncProperty(Task<T> loader)
        {
            if (loader != null)
            {
#pragma warning disable 4014
                Load(loader);
#pragma warning restore 4014
            }
        }

        public ProgressProperty Progress { get; } = new ProgressProperty();

        public async Task Load(Func<IProgress<ProgressData>,Task<T>> loaderDelegate)
        {                
            var progress = new Infrastructure.Progress.Progress();

            EventHandler<ProgressData> eventHandler = (_, data) =>
            {
                this.Progress.Progress = data.Progress;
                this.Progress.Message = data.Message;
            };

            progress.ProgressChanged += eventHandler;

            try
            {
                this.Progress.Error = null;
                await this.Load(loaderDelegate(progress)).ConfigureAwait(false);
            }
            finally
            {
                progress.ProgressChanged -= eventHandler;
            }
        }

        public Task Load(Func<Task<T>> loaderDelegate)
        {
            return this.Load(loaderDelegate());
        }

        public async Task Load(Task<T> loader)
        {
            try
            {
                this.Progress.IsLoading = true;
                var newValue = await loader.ConfigureAwait(true);

                if (CurrentValue != null && typeof(T).IsGenericType)
                {
                    var generic = typeof(T).GetGenericTypeDefinition();

                    if (typeof(ObservableCollection<>) == generic)
                    {
                        // this is an observable collection, use events
                        if (CurrentValue == null)
                        {
                            CurrentValue = (T)Activator.CreateInstance(typeof(ObservableCollection<>).MakeGenericType(typeof(T).GetGenericArguments()[0]));
                        }
                        else
                        {
                            ((IList)CurrentValue).Clear();
                        }

                        Debug.Assert(CurrentValue is IList);

                        foreach (var item in (IList)newValue)
                        {
                            ((IList)CurrentValue).Add(item);
                        }

                        HasValue = true;
                        OnLoaded(EventArgs.Empty);
                        return;
                    }
                }

                HasValue = true;
                CurrentValue = newValue;

                this.Progress.HasError = false;
                this.Progress.Error = null;
                OnLoaded(EventArgs.Empty);
            }
            catch (Exception e)
            {
                this.Progress.Error = e.Message;
                this.Progress.HasError = !string.IsNullOrEmpty(e.Message);
            }
            finally
            {
                this.Progress.IsLoading = false;
            }
        }
        
        public T CurrentValue
        {
            get => currentValue;
            private set
            {
                SetField(ref currentValue, value);
                OnPropertyChanged(nameof(HasValue));
            }
        }

        public bool HasValue { get; private set; }
        
        public event EventHandler<EventArgs> Loaded;

        protected void OnLoaded(EventArgs args)
        {
            Loaded?.Invoke(this, args);
        }
    }
}
