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
using System.ComponentModel;
using System.Linq;

namespace Otor.MsixHero.App.Mvvm.Changeable
{
    public class ValidatedChangeableCollection<T> : ChangeableCollection<T>, IValidatedContainerChangeable, IDataErrorInfo
    {
        private string validationMessage;
        private bool isValidated = true;
        private IReadOnlyCollection<Func<IEnumerable<T>, string>> validators;
        private bool suppressValidation;

        public ValidatedChangeableCollection(Func<IEnumerable<T>, string> validator = null)
        {
            if (validator != null)
            {
                this.validators = new List<Func<IEnumerable<T>, string>> { validator };
            }
        }

        public ValidatedChangeableCollection(Func<IEnumerable<T>, string> validator, IEnumerable<T> items) : base(items)
        {
            if (validator != null)
            {
                this.validators = new List<Func<IEnumerable<T>, string>> { validator };
            }
        }

        public void AddRange(IEnumerable<T> items)
        {
            var suppress = this.suppressValidation;
            
            try
            {
                this.suppressValidation = true;
                
                foreach (var item in items)
                {
                    this.Add(item);
                }
            }
            finally
            {
                this.suppressValidation = suppress;
                this.Validate();
            }
        }

        public string Error => this.ValidationMessage;

        public string this[string columnName] => null;

        protected override void ClearItems()
        {
            base.ClearItems();
            this.Validate();
        }

        protected override void InsertItem(int index, T item)
        {
            base.InsertItem(index, item);

            if (item is IValidatedChangeable validatedItem)
            {
                validatedItem.ValidationStatusChanged -= this.ItemOnValidationStatusChanged;
                validatedItem.ValidationStatusChanged += this.ItemOnValidationStatusChanged;
            }

            this.Validate();
        }

        private void ItemOnValidationStatusChanged(object sender, ValueChangedEventArgs<string> e)
        {
            if (string.IsNullOrEmpty(e.NewValue))
            {
                this.Validate();
            }
            else
            {
                this.Validate(e.NewValue);
            }
        }

        protected override void MoveItem(int oldIndex, int newIndex)
        {
            base.MoveItem(oldIndex, newIndex);
            this.Validate();
        }

        protected override void RemoveItem(int index)
        {
            var item = this[index];
            if (item is IValidatedChangeable validatedItem)
            {
                validatedItem.ValidationStatusChanged -= this.ItemOnValidationStatusChanged;
            }

            base.RemoveItem(index);
            this.Validate();
        }

        protected override void SetItem(int index, T item)
        {
            var oldItem = this[index];
            if (oldItem is IValidatedChangeable oldValidatedItem)
            {
                oldValidatedItem.ValidationStatusChanged -= this.ItemOnValidationStatusChanged;
            }

            base.SetItem(index, item);

            if (item is IValidatedChangeable newValidatedItem)
            {
                newValidatedItem.ValidationStatusChanged += this.ItemOnValidationStatusChanged;
            }
            
            this.Validate();
        }

        public string ValidationMessage
        {
            get => this.validationMessage;
            private set
            {
                var oldIsValid = string.IsNullOrEmpty(this.validationMessage);
                if (!this.SetField(ref this.validationMessage, value))
                {
                    return;
                }

                var newIsValid = string.IsNullOrEmpty(value);
                if (oldIsValid != newIsValid)
                {
                    this.OnPropertyChanged(new PropertyChangedEventArgs(nameof(this.IsValid)));
                }

                this.OnPropertyChanged(new PropertyChangedEventArgs(nameof(this.Error)));
            }
        }

        public bool IsValidated
        {
            get => this.isValidated;
            set
            {
                if (!this.SetField(ref this.isValidated, value))
                {
                    return;
                }

                this.Validate();
            }
        }

        public bool IsValid => string.IsNullOrEmpty(this.validationMessage);
        
        public IReadOnlyCollection<Func<IEnumerable<T>, string>> Validators
        {
            get => this.validators;
            set
            {
                this.validators = value;
                this.Validate();
            }
        }

        public event EventHandler<ValueChangedEventArgs<string>> ValidationStatusChanged;
        
        private void Validate(string errorMessage = null)
        {
            if (this.suppressValidation)
            {
                return;
            }
            
            var oldValidationMessage = this.ValidationMessage;

            if (!string.IsNullOrEmpty(errorMessage))
            {
                this.ValidationMessage = errorMessage;
            }
            else
            {
                if (!this.IsValidated || this.Validators == null || !this.Validators.Any())
                {
                    this.ValidationMessage = null;
                }
                else
                {
                    string msg = null;
                    foreach (var validator in this.Validators)
                    {
                        msg = validator(this);
                        if (!string.IsNullOrEmpty(msg))
                        {
                            break;
                        }
                    }

                    this.ValidationMessage = msg;
                }
                
                if (string.IsNullOrEmpty(this.ValidationMessage))
                {
                    foreach (var item in this.Items.OfType<IValidatedChangeable>().Where(x => x.IsValidated))
                    {
                        this.ValidationMessage = item.ValidationMessage;
                        if (!string.IsNullOrEmpty(this.ValidationMessage))
                        {
                            break;
                        }
                    }
                }
            }

            // ReSharper disable once InvertIf
            if (oldValidationMessage != this.ValidationMessage)
            {
                var validationChanged = this.ValidationStatusChanged;
                if (validationChanged != null)
                {
                    validationChanged(this, new ValueChangedEventArgs<string>(this.ValidationMessage));
                }
            }
        }
    }
}