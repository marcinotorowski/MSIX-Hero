using System;
using System.ComponentModel;

namespace otor.msixhero.ui.Domain
{
    public class ValidatedChangeableProperty<T> : ChangeableProperty<T>, IValidatedChangeable<T>, IDataErrorInfo
    {
        private string validationMessage;
        private bool isValidated;
        private Func<T, string> validator;

        public ValidatedChangeableProperty(T initialValue = default) : this(null, initialValue)
        {
        }

        public ValidatedChangeableProperty(Func<T, string> validator, T initialValue = default) : this(validator, true, initialValue)
        {
            this.validator = validator;
            this.Validate();
        }

        public ValidatedChangeableProperty(Func<T, string> validator, bool isValidated, T initialValue = default) : base(initialValue)
        {
            this.isValidated = isValidated;
            this.validator = validator;
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
                    this.OnPropertyChanged(nameof(this.IsValid));
                    this.OnPropertyChanged(nameof(Error));
                }
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

        public Func<T, string> Validator
        {
            get => this.validator;
            set
            {
                this.validator = value;
                this.Validate();
            }
        }

        public string Error => this.ValidationMessage;

        public string this[string columnName]
        {
            get
            {
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
            if (!this.IsValidated || this.Validator == null)
            {
                this.ValidationMessage = null;
            }
            else
            {
                this.ValidationMessage = this.Validator(this.CurrentValue);
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