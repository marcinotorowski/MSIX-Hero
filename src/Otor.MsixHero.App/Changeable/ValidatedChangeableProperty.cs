using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace Otor.MsixHero.App.Changeable
{
    public class ValidatedChangeableProperty<T> : ChangeableProperty<T>, IValidatedChangeable<T>, IDataErrorInfo
    {
        // ReSharper disable once InconsistentNaming
        private string validationMessage;
        private bool isValidated;
        private IReadOnlyCollection<Func<T, string>> validators;
        private bool displayValidationErrors = true;
        private string displayName;

        public ValidatedChangeableProperty(string displayName, T initialValue = default) : base(initialValue)
        {
            this.displayName = displayName;
            this.isValidated = true;
            this.validators = new Func<T, string>[0];
            this.Validate();
        }

        public ValidatedChangeableProperty(string displayName, T initialValue, params Func<T, string>[] validators) : this(displayName, initialValue, true, validators)
        {
        }

        public ValidatedChangeableProperty(string displayName, params Func<T, string>[] validators) : this(displayName, default, true, validators)
        {
        }

        public ValidatedChangeableProperty(string displayName, T initialValue, bool isValidated, params Func<T, string>[] validators) : base(initialValue)
        {
            this.displayName = displayName;
            this.isValidated = isValidated;
            this.validators = validators;
            this.Validate();
        }

        public ValidatedChangeableProperty(string displayName, bool isValidated, params Func<T, string>[] validators) : this(displayName, default, isValidated, validators)
        {
        }

        public string DisplayName
        {
            get => this.displayName;
            set
            {
                if (!this.SetField(ref this.displayName, value) || !this.isValidated)
                {
                    return;
                }

                this.Validate();
            }
        }

        public bool DisplayValidationErrors
        {
            get => this.displayValidationErrors;
            set
            {
                if (!this.SetField(ref this.displayValidationErrors, value))
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

        public IReadOnlyCollection<Func<T, string>> Validators
        {
            get => this.validators;
            set
            {
                this.validators = value;
                this.Validate();
            }
        }

        public string Error
        {
            get
            {
                if (!this.DisplayValidationErrors)
                {
                    return null;
                }

                return this.ValidationMessage;
            }
        }

        public string this[string columnName]
        {
            get
            {
                if (!this.displayValidationErrors)
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

        public event EventHandler<ValueChangedEventArgs<string>> ValidationStatusChanged;

        protected override void PostSetValue()
        {
            base.PostSetValue();
            this.Validate();
        }

        private void Validate()
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
                if (validationChanged != null)
                {
                    validationChanged(this, new ValueChangedEventArgs<string>(this.ValidationMessage));
                }
            }

            this.OnPropertyChanged(nameof(this.CurrentValue));
            this.OnPropertyChanged(nameof(this.Error));
            this.OnPropertyChanged(nameof(this.IsValid));
        }
    }
}