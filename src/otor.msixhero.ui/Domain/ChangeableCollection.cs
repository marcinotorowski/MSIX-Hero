using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;

namespace otor.msixhero.ui.Domain
{
    public class ChangeableCollection<T> : ObservableCollection<T>, IChangeable
    {
        private bool isTouched, isDirty;

        private bool monitorChildren = true;

        private List<T> originalItems = new List<T>();
        
        public ChangeableCollection()
        {
            this.AssertType();
        }

        public ChangeableCollection(IEnumerable<T> collection)
        {
            this.AssertType();
            try
            {
                this.monitorChildren = false;
                this.AddRange(collection);
                this.originalItems = this.ToList();
            }
            finally
            {
                this.monitorChildren = true;
            }
        }

        public ChangeableCollection(params T[] collection) : this((IEnumerable<T>)collection)
        {
        }

        protected override void ClearItems()
        {
            base.ClearItems();
            this.IsTouched = true;
            this.IsDirty = !this.originalItems.SequenceEqual(this);
        }

        protected override void InsertItem(int index, T item)
        {
            base.InsertItem(index, item);

            if (item is IChangeableValue newChangeable)
            {
                newChangeable.ValueChanged -= this.OnItemValueChanged;
                newChangeable.ValueChanged += this.OnItemValueChanged;
            }

            if (!this.monitorChildren)
            {
                return;
            }

            this.IsTouched = true;

            if (item is IChangeable changeableItem)
            {
                var isNewDirty = changeableItem.IsDirty;
                this.IsDirty = isNewDirty || !this.originalItems.SequenceEqual(this);
            }
            else
            {
                this.IsDirty = !this.originalItems.SequenceEqual(this);
            }
        }

        protected override void MoveItem(int oldIndex, int newIndex)
        {
            base.MoveItem(oldIndex, newIndex);

            if (!this.monitorChildren)
            {
                return;
            }

            this.IsTouched = true;
            this.IsDirty = !this.originalItems.SequenceEqual(this);
        }

        protected override void RemoveItem(int index)
        {
            var item = this[index];
            base.RemoveItem(index);

            if (item is IChangeableValue oldChangeable)
            {
                oldChangeable.ValueChanged -= this.OnItemValueChanged;
            }

            if (!this.monitorChildren)
            {
                return;
            }

            this.IsTouched = true;

            if (item is IChangeable)
            {
                this.IsDirty = !this.originalItems.SequenceEqual(this) || this.OfType<IChangeable>().Any(x => x.IsDirty);
            }
            else
            {
                this.IsDirty = !this.originalItems.SequenceEqual(this);
            }
        }

        protected override void SetItem(int index, T item)
        {
            var oldItem = this[index];
            base.SetItem(index, item);

            if (oldItem is IChangeableValue oldChangeable)
            {
                oldChangeable.ValueChanged -= this.OnItemValueChanged;
            }

            if (item is IChangeableValue newChangeable)
            {
                newChangeable.ValueChanged -= this.OnItemValueChanged;
                newChangeable.ValueChanged += this.OnItemValueChanged;
            }

            if (!this.monitorChildren)
            {
                return;
            }

            this.IsTouched = true;

            if (item is IChangeable changeableItem)
            {
                var oldIsDirty = ((IChangeable) oldItem).IsDirty;
                var newIsDirty = changeableItem.IsDirty;
                if (oldIsDirty != newIsDirty)
                {
                    this.IsDirty = newIsDirty || !this.originalItems.SequenceEqual(this);
                }
            }
            else
            {
                this.IsDirty = !this.originalItems.SequenceEqual(this);
            }
        }

        private void OnItemValueChanged(object? sender, ValueChangedEventArgs e)
        {
            if (!this.monitorChildren)
            {
                return;
            }

            this.IsTouched = true;
            var actualSender = (IChangeable) sender;
            if (actualSender != null)
            {
                if (actualSender.IsDirty)
                {
                    this.IsDirty = true;
                }
                else
                {
                    this.IsDirty = !this.originalItems.SequenceEqual(this) || this.OfType<IChangeable>().Any(i => i.IsDirty);
                }
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
            get => this.isTouched;
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

        public void Commit()
        {
            try
            {
                this.monitorChildren = false;

                foreach (var item in this.OfType<IChangeable>())
                {
                    item.Commit();
                }
            }
            finally
            {
                this.monitorChildren = true;
                this.IsDirty = false;
                this.IsTouched = false;
                this.originalItems = this.ToList();
            }
        }

        public void Reset(ValueResetType resetType = ValueResetType.Hard)
        {
            try
            {
                this.monitorChildren = false;

                this.ClearItems();
                if (this.originalItems?.Any() == true)
                {
                    this.AddRange(this.originalItems);

                    foreach (var item in this.originalItems.OfType<IChangeable>())
                    {
                        item.Reset();
                    }
                }
            }
            finally
            {
                this.monitorChildren = true;
            }

            this.IsDirty = false;

            if (resetType == ValueResetType.Hard)
            {
                this.IsTouched = false;
            }
        }

        public void Touch()
        {
            this.IsTouched = true;
        }

        public event EventHandler<ValueChangedEventArgs<bool>> IsDirtyChanged;

        public event EventHandler<ValueChangedEventArgs<bool>> IsTouchedChanged;
        
        protected bool SetField<T1>(ref T1 field, T1 value, [CallerMemberName] string propertyName = null)
        {
            if (EqualityComparer<T1>.Default.Equals(field, value))
            {
                return false;
            }

            field = value;
            this.OnPropertyChanged(new PropertyChangedEventArgs(propertyName));
            return true;
        }
        private void AssertType()
        {
            if (typeof(IChangeable).IsAssignableFrom(typeof(T)))
            {
                return;
            }

            if (typeof(T) == typeof(string))
            {
                return;
            }

            if (typeof(T).IsPrimitive || typeof(T).IsValueType)
            {
                return;
            }

            throw new NotSupportedException("Objects supported by this class must be strings, value types or they must implement IChangeable interface.");
        }
    }
}
