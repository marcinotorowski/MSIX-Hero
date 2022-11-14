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
using System.Globalization;
using System.Linq;
using Otor.MsixHero.Infrastructure.Localization;

namespace Otor.MsixHero.App.Mvvm.Changeable
{
    public class ValidatedChangeableProperty<T> : ChangeableProperty<T>, IValidatedChangeable<T>, IDataErrorInfo
    {
        // ReSharper disable once InconsistentNaming
        private string validationMessage;
        private bool _isValidated;
        private IReadOnlyCollection<Func<T, string>> _validators;
        private bool _displayValidationErrors = true;
        private readonly Func<string> _displayName;

        public ValidatedChangeableProperty(string displayName, T initialValue = default) : base(initialValue)
        {
            this._displayName = () => displayName;
            this._isValidated = true;
            this._validators = Array.Empty<Func<T, string>>();
            this.Validate();
            MsixHeroTranslation.Instance.CultureChanged += this.InstanceOnCultureChanged;
        }

        public ValidatedChangeableProperty(string displayName, T initialValue, params Func<T, string>[] validators) : this(displayName, initialValue, true, validators)
        {
        }

        public ValidatedChangeableProperty(string displayName, params Func<T, string>[] validators) : this(displayName, default, true, validators)
        {
        }

        public ValidatedChangeableProperty(string displayName, T initialValue, bool isValidated, params Func<T, string>[] validators) : base(initialValue)
        {
            this._displayName = () => displayName;
            this._isValidated = isValidated;
            this._validators = validators;
            this.Validate();
            MsixHeroTranslation.Instance.CultureChanged += this.InstanceOnCultureChanged;
        }

        public ValidatedChangeableProperty(Func<string> displayName, bool isValidated, params Func<T, string>[] validators) : this(displayName, default, isValidated, validators)
        {
        }

        public ValidatedChangeableProperty(Func<string> displayName, T initialValue = default) : base(initialValue)
        {
            this._displayName = displayName;
            this._isValidated = true;
            this._validators = Array.Empty<Func<T, string>>();
            this.Validate();
            MsixHeroTranslation.Instance.CultureChanged += this.InstanceOnCultureChanged;
        }

        public ValidatedChangeableProperty(Func<string> displayName, T initialValue, params Func<T, string>[] validators) : this(displayName, initialValue, true, validators)
        {
        }

        public ValidatedChangeableProperty(Func<string> displayName, params Func<T, string>[] validators) : this(displayName, default, true, validators)
        {
        }

        public ValidatedChangeableProperty(Func<string> displayName, T initialValue, bool isValidated, params Func<T, string>[] validators) : base(initialValue)
        {
            this._displayName = displayName;
            this._isValidated = isValidated;
            this._validators = validators;
            this.Validate();
            MsixHeroTranslation.Instance.CultureChanged += this.InstanceOnCultureChanged;
        }

        public ValidatedChangeableProperty(string displayName, bool isValidated, params Func<T, string>[] validators) : this(displayName, default, isValidated, validators)
        {
        }

        public string DisplayName => this._displayName();

        public bool DisplayValidationErrors
        {
            get => this._displayValidationErrors;
            set
            {
                if (!this.SetField(ref this._displayValidationErrors, value))
                {
                    return;
                }

                this.OnPropertyChanged(nameof(this.Error));
                this.OnPropertyChanged(nameof(this.CurrentValue));
            }
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
                    this.OnPropertyChanged(nameof(this.IsValid));
                }

                this.OnPropertyChanged(nameof(Error));
            }
        }

        public bool IsValidated
        {
            get => this._isValidated;
            set
            {
                if (!this.SetField(ref this._isValidated, value))
                {
                    return;
                }

                this.Validate();
            }
        }

        public bool IsValid => string.IsNullOrEmpty(this.validationMessage);

        public IReadOnlyCollection<Func<T, string>> Validators
        {
            get => this._validators;
            set
            {
                this._validators = value;
                this.Validate();
            }
        }

        public string Error => this.ValidationMessage;

        public string this[string columnName]
        {
            get
            {
                if (!this._displayValidationErrors)
                {
                    return null;
                }

                switch (columnName)
                {
                    case nameof(CurrentValue):
                        return this.ValidationMessage;
                    default:
                        return null;
                }
            }
        }

        public void Validate()
        {
            var oldValidationMessage = this.ValidationMessage;
            if (!this.IsValidated || this.Validators == null || !this.Validators.Any())
            {
                this.ValidationMessage = null;
            }
            else
            {
                string msg = null;
                foreach (var validator in this.Validators)
                {
                    msg = validator(this.CurrentValue);
                    if (!string.IsNullOrEmpty(msg))
                    {
                        break;
                    }
                }

                if (string.IsNullOrEmpty(msg))
                {
                    this.ValidationMessage = null;
                }
                else if (this.DisplayName != null)
                {
                    this.ValidationMessage = this.DisplayName + ": " + msg;
                }
                else
                {
                    this.ValidationMessage = msg;
                }
            }

            // ReSharper disable once InvertIf
            if (oldValidationMessage != this.ValidationMessage)
            {
                var validationChanged = this.ValidationStatusChanged;
                validationChanged?.Invoke(this, new ValueChangedEventArgs<string>(this.ValidationMessage));
            }

            this.OnPropertyChanged(nameof(this.CurrentValue));
            this.OnPropertyChanged(nameof(this.Error));
            this.OnPropertyChanged(nameof(this.IsValid));
        }

        public event EventHandler<ValueChangedEventArgs<string>> ValidationStatusChanged;

        protected override void PostSetValue()
        {
            base.PostSetValue();
            this.Validate();
        }
        private void InstanceOnCultureChanged(object sender, CultureInfo e)
        {
            this.Validate();
            this.OnPropertyChanged(nameof(this.DisplayName));
        }
    }
}