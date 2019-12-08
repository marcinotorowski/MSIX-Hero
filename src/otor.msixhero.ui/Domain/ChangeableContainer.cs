using System;
using System.Collections.Generic;
using System.Linq;
using otor.msixhero.ui.ViewModel;

namespace otor.msixhero.ui.Domain
{
    public class ChangeableContainer : NotifyPropertyChanged, IDisposable, IValidatedChangeable
    {
        private readonly HashSet<IChangeable> children = new HashSet<IChangeable>();
        private readonly HashSet<IChangeable> touchedChildren = new HashSet<IChangeable>();
        private readonly HashSet<IValidatedChangeable> invalidChildren = new HashSet<IValidatedChangeable>();
        private readonly HashSet<IChangeable> dirtyChildren = new HashSet<IChangeable>();
        private bool isDirty;
        private bool isTouched;
        private bool suppressListening;
        private bool isValidated;
        private string validationMessage;

        public ChangeableContainer()
        {
        }

        public ChangeableContainer(params IChangeable[] initialChildren)
        {
            foreach (var item in initialChildren)
            {
                if (!this.children.Add(item))
                {
                    continue;
                }

                item.IsDirtyChanged += this.OnSubItemDirtyChanged;
                item.IsTouchedChanged += this.OnSubItemTouchedChanged;

                if (item is IValidatedChangeable changeableValue)
                {
                    changeableValue.ValidationStatusChanged += this.OnSubItemValidationStatusChanged;
                }

                if (item.IsTouched)
                {
                    this.touchedChildren.Add(item);
                }

                if (item.IsDirty)
                {
                    this.dirtyChildren.Add(item);
                }

                if (item is IValidatedChangeable validatedItem)
                {
                    this.isValidated = true;

                    if (!validatedItem.IsValid)
                    {
                        this.invalidChildren.Add(validatedItem);
                    }
                }
            }

            this.isDirty = this.dirtyChildren.Any();
            this.isTouched = this.touchedChildren.Any();
        }

        public bool IsDirty
        {
            get => this.isDirty;
            private set
            {
                if (this.isDirty == value)
                {
                    return;
                }

                this.SetField(ref this.isDirty, value);

                var dirtyChanged = this.IsDirtyChanged;
                if (dirtyChanged == null)
                {
                    return;
                }

                dirtyChanged(this, new ValueChangedEventArgs<bool>(value));
            }
        }

        public bool IsTouched
        {
            get => this.isTouched;
            private set
            {
                if (this.isTouched == value)
                {
                    return;
                }

                this.SetField(ref this.isTouched, value);

                var touchedChanged = this.IsTouchedChanged;
                if (touchedChanged == null)
                {
                    return;
                }

                touchedChanged(this, new ValueChangedEventArgs<bool>(value));
            }
        }

        public IReadOnlyCollection<IChangeable> TouchedChildren => this.touchedChildren;

        public IReadOnlyCollection<IChangeable> DirtyChildren => this.dirtyChildren;

        public IReadOnlyCollection<IValidatedChangeable> InvalidChildren => this.invalidChildren;
        
        public event EventHandler<ValueChangedEventArgs<bool>> IsDirtyChanged;

        public event EventHandler<ValueChangedEventArgs<bool>> IsTouchedChanged;

        public event EventHandler<ValueChangedEventArgs<string>> ValidationStatusChanged;

        public void Commit()
        {
            try
            {
                this.suppressListening = true;

                foreach (var item in this.TouchedChildren)
                {
                    item.Commit();
                }

                this.IsTouched = false;
                this.IsDirty = false;
            }
            finally
            {
                this.suppressListening = false;
            }
        }

        public void Reset(ValueResetType resetType = ValueResetType.Hard)
        {
            try
            {
                this.suppressListening = true;

                var isAnyTouched = false;

                foreach (var item in this.TouchedChildren)
                {
                    item.Reset(resetType);
                    isAnyTouched |= item.IsTouched;
                }

                this.IsTouched = isAnyTouched;
                this.IsDirty = false;
            }
            finally
            {
                this.suppressListening = false;
            }
        }

        public void Touch()
        {
            this.IsTouched = true;
        }

        public void AddChild(IChangeable item)
        {
            this.AddChildren(item);
        }

        public void AddChildren(params IChangeable[] childrenToAdd)
        {
            foreach (var item in childrenToAdd)
            {
                if (!this.children.Add(item))
                {
                    continue;
                }

                item.IsDirtyChanged += this.OnSubItemDirtyChanged;
                item.IsTouchedChanged += this.OnSubItemTouchedChanged;

                if (item is IValidatedChangeable changeableValue)
                {
                    changeableValue.ValidationStatusChanged += this.OnSubItemValidationStatusChanged;
                }

                if (item.IsTouched)
                {
                    this.touchedChildren.Add(item);
                }

                if (item.IsDirty)
                {
                    this.dirtyChildren.Add(item);
                }

                // ReSharper disable once InvertIf
                if (item is IValidatedChangeable validatedItem)
                {
                    var wasValidated = this.isValidated;
                    this.isValidated = true;

                    if (!validatedItem.IsValid)
                    { 
                        this.invalidChildren.Add(validatedItem);

                        if (this.ValidationMessage != null)
                        {
                            this.ValidationMessage = validatedItem.ValidationMessage;
                        }
                    }

                    if (wasValidated != this.isValidated)
                    {
                        this.OnPropertyChanged(nameof(isValidated));
                    }
                }
            }

            this.IsDirty |= this.dirtyChildren.Any();
            this.IsTouched |= this.touchedChildren.Any();
        }

        public void Dispose()
        {
            foreach (var item in this.children)
            {
                item.IsDirtyChanged -= this.OnSubItemDirtyChanged;
                item.IsTouchedChanged -= this.OnSubItemTouchedChanged;

                if (item is IValidatedChangeable changeableValue)
                {
                    changeableValue.ValidationStatusChanged -= this.OnSubItemValidationStatusChanged;
                }
            }

            this.children.Clear();
            this.dirtyChildren.Clear();
            this.invalidChildren.Clear();
            this.touchedChildren.Clear();

            this.IsDirty = false;
            this.IsTouched = false;

            this.ValidationMessage = null;
            this.isValidated = false;
            this.OnPropertyChanged(nameof(isValidated));
        }

        public string ValidationMessage
        {
            get => this.validationMessage;
            set
            {
                if (!this.SetField(ref this.validationMessage, value))
                {
                    return;
                }

                this.OnPropertyChanged(nameof(IsValid));
                var validationStatusChanged = this.ValidationStatusChanged;
                validationStatusChanged?.Invoke(this, new ValueChangedEventArgs<string>(value));
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

                this.invalidChildren.Clear();

                foreach (var item in this.children.OfType<IValidatedChangeable>())
                {
                    if (item.IsValidated == value)
                    {
                        continue;
                    }

                    item.IsValidated = value;
                }

                if (value)
                {
                    foreach (var item in this.children.OfType<IValidatedChangeable>().Where(item => !item.IsValid))
                    {
                        this.invalidChildren.Add(item);
                    }

                    this.ValidationMessage = this.invalidChildren.FirstOrDefault()?.ValidationMessage;
                }
                else
                {
                    this.ValidationMessage = null;
                }
            }
        }

        public bool IsValid => string.IsNullOrEmpty(this.ValidationMessage);

        private void OnSubItemValidationStatusChanged(object sender, ValueChangedEventArgs<string> e)
        {
            if (this.suppressListening)
            {
                return;
            }
            
            if (string.IsNullOrEmpty(e.NewValue))
            {
                this.invalidChildren.Remove((IValidatedChangeable)sender);
            }
            else
            {
                this.invalidChildren.Add((IValidatedChangeable)sender);
            }

            this.ValidationMessage = this.invalidChildren.FirstOrDefault()?.ValidationMessage;
        }

        private void OnSubItemDirtyChanged(object sender, ValueChangedEventArgs<bool> e)
        {
            if (this.suppressListening)
            {
                return;
            }

            if (e.NewValue)
            {
                this.IsDirty = true;
                this.dirtyChildren.Add((IChangeable)sender);
            }
            else
            {
                this.dirtyChildren.Remove((IChangeable)sender);
                this.IsDirty = this.dirtyChildren.Any(d => d.IsDirty);
            }
        }

        private void OnSubItemTouchedChanged(object sender, ValueChangedEventArgs<bool> e)
        {
            if (this.suppressListening)
            {
                return;
            }

            if (e.NewValue)
            {
                this.IsTouched = true;
                this.touchedChildren.Add((IChangeable)sender);
            }
            else
            {
                this.touchedChildren.Remove((IChangeable)sender);
                this.IsTouched = this.touchedChildren.Any(d => d.IsTouched);
            }
        }
    }
}