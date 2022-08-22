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
        private bool isLoading;
        private bool hasError;
        private string error;

        public AsyncProperty(T initialValue = default, bool isLoading = false)
        {
            this.isLoading = isLoading;
            currentValue = initialValue;
        }

        public AsyncProperty(Task<T> loader, IProgress<ProgressData> progressReporter = null)
        {
            if (loader != null)
            {
#pragma warning disable 4014
                Load(loader, progressReporter);
#pragma warning restore 4014
            }
        }

        public async Task Load(Task<T> loader, IProgress<ProgressData> progressReporter = null)
        {
            try
            {
                IsLoading = true;
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

                HasError = false;
                Error = null;
                OnLoaded(EventArgs.Empty);
            }
            catch (Exception e)
            {
                Error = e.Message;
                HasError = !string.IsNullOrEmpty(e.Message);
            }
            finally
            {
                IsLoading = false;
            }
        }

        public bool HasError
        {
            get => hasError;
            private set => SetField(ref hasError, value);
        }

        public string Error
        {
            get => error;
            private set => SetField(ref error, value);
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

        public bool IsLoading
        {
            get => isLoading;
            private set => SetField(ref isLoading, value);
        }

        public event EventHandler<EventArgs> Loaded;

        protected void OnLoaded(EventArgs args)
        {
            Loaded?.Invoke(this, args);
        }
    }
}
