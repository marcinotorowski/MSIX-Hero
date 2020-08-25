using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace Otor.MsixHero.Ui.Domain
{
    public class ValidatedChangeableProperty<T> : ChangeableProperty<T>, IValidatedChangeable<T>, IDataErrorInfo
    {
        // ReSharper disable once InconsistentNaming
        private static Func<T, string> validateNotNull;
        private string validationMessage;
        private bool isValidated;
        private IReadOnlyCollection<Func<T, string>> validators;
        private ValidationMode validationMode;

        public ValidatedChangeableProperty(T initialValue = default) : base(initialValue)
        {
            this.isValidated = true;
            this.validators = new Func<T, string>[0];
            this.Validate();
        }

        public ValidatedChangeableProperty(T initialValue, params Func<T, string>[] validators) : this(initialValue, true, validators)
        {
        }

        public ValidatedChangeableProperty(params Func<T, string>[] validators) : this(default, true, validators)
        {
        }

        public ValidatedChangeableProperty(T initialValue, bool isValidated, params Func<T, string>[] validators) : base(initialValue)
        {
            this.isValidated = isValidated;
            this.validators = validators;
            this.Validate();
        }

        public ValidatedChangeableProperty(bool isValidated, params Func<T, string>[] validators) : this(default, isValidated, validators)
        {
        }
        
        public ValidationMode ValidationMode
        {
            get => this.validationMode;
            set
            {
                if (!this.SetField(ref this.validationMode, value))
                {
                    return;
                }

                this.Validate();
            }
        }

        public static Func<T, string> ValidateNotNull
        {
            get => validateNotNull ??= value =>
            {
                if (typeof(T) == typeof(string))
                {
                    if (string.IsNullOrEmpty(value as string))
                    {
                        return "This value may not be empty.";
                    }
                }
                else if (!typeof(T).IsValueType)
                {
                    if (value == null)
                    {
                        return "This value is required.";
                    }
                }

                return null;
            };
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
                if (this.validationMode == ValidationMode.Silent)
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
                if (this.validationMode == ValidationMode.Silent)
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

                this.ValidationMessage = msg;
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