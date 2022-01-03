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
using System.Collections.Generic;

namespace Otor.MsixHero.App.Mvvm.Changeable
{
    public class ChangeableProperty<T> : NotifyPropertyChanged, IChangeable<T>
    {
        private T currentValue;
        private bool isTouched;
        private bool isDirty;

        public ChangeableProperty(T initialValue = default)
        {
            this.OriginalValue = initialValue;
            this.currentValue = initialValue;
            // ReSharper disable once VirtualMemberCallInConstructor
            this.PostSetValue();
        }

        public bool HasValue
        {
            get
            {
                if (this.OriginalValue is IEquatable<T> equatable)
                {
                    return !equatable.Equals(default);
                }

                return !EqualityComparer<T>.Default.Equals(default, this.currentValue);
            }
        }

        public T OriginalValue { get; private set; }

        public T CurrentValue
        {
            get => this.currentValue;
            set
            {
                var wasDirty = this.isDirty;
                var wasTouched = this.isTouched;
                var oldValue = this.currentValue;

                bool newIsDirty;

                if (value is IEquatable<T> equatable)
                {
                    if (equatable.Equals(this.currentValue))
                    {
                        return;
                    }

                    newIsDirty = !equatable.Equals(this.OriginalValue);
                }
                else
                {
                    if (EqualityComparer<T>.Default.Equals(value, this.currentValue))
                    {
                        return;
                    }

                    newIsDirty = !EqualityComparer<T>.Default.Equals(value, this.OriginalValue);
                }

                var valueChanging = this.ValueChanging;
                if (valueChanging != null)
                {
                    var args = new ValueChangingEventArgs(oldValue, value, this.OriginalValue, wasDirty, wasTouched);
                    valueChanging(this, args);

                    if (args.Cancel)
                    {
                        return;
                    }
                }

                this.IsDirty = newIsDirty;
                this.IsTouched = true;

                if (!this.SetField(ref this.currentValue, value))
                {
                    return;
                }

                this.OnPropertyChanged(nameof(HasValue));
                this.PostSetValue();

                var valueChanged = this.ValueChanged;
                if (valueChanged != null)
                {
                    var args = new ValueChangedEventArgs(oldValue, value, this.OriginalValue, wasDirty, wasTouched);
                    valueChanged(this, args);
                }

                this.Changed?.Invoke(this, EventArgs.Empty);
            }
        }

        public bool IsDirty
        {
            get => this.isDirty;
            private set
            {
                if (!this.SetField(ref this.isDirty, value))
                {
                    return;
                }

                var isDirtyChanged = this.IsDirtyChanged;
                if (isDirtyChanged != null)
                {
                    isDirtyChanged(this, new ValueChangedEventArgs<bool>(value));
                }
            }
        }

        public bool IsTouched
        {
            get => isTouched;
            private set
            {
                if (!this.SetField(ref this.isTouched, value))
                {
                    return;
                }

                var isTouchedChanged = this.IsTouchedChanged;
                if (isTouchedChanged != null)
                {
                    isTouchedChanged(this, new ValueChangedEventArgs<bool>(value));
                }
            }
        }

        public event EventHandler<EventArgs> Changed;

        public event EventHandler<ValueChangedEventArgs> ValueChanged;

        public event EventHandler<ValueChangingEventArgs> ValueChanging;

        public event EventHandler<ValueChangedEventArgs<bool>> IsDirtyChanged;

        public event EventHandler<ValueChangedEventArgs<bool>> IsTouchedChanged;

        public void Commit()
        {
            this.OriginalValue = this.CurrentValue;
            this.IsDirty = false;
            this.IsTouched = false;
        }

        public void Touch()
        {
            this.IsTouched = true;
        }

        public virtual void Reset(ValueResetType resetType = ValueResetType.Hard)
        {
            var currentIsTouched = this.IsTouched;
            this.CurrentValue = this.OriginalValue;

            if (currentIsTouched && resetType == ValueResetType.Hard)
            {
                this.IsTouched = false;
            }
        }

        protected virtual void PostSetValue()
        {
        }
    }
}
