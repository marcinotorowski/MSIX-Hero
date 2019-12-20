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

        private List<T> originalItems = new List<T>();

        protected override void ClearItems()
        {
            base.ClearItems();
            this.IsTouched = true;
            this.IsDirty = !this.originalItems.SequenceEqual(this);
        }

        protected override void InsertItem(int index, T item)
        {
            base.InsertItem(index, item);
            this.IsTouched = true;
            this.IsDirty = !this.originalItems.SequenceEqual(this);
        }

        protected override void MoveItem(int oldIndex, int newIndex)
        {
            base.MoveItem(oldIndex, newIndex);
            this.IsTouched = true;
            this.IsDirty = !this.originalItems.SequenceEqual(this);
        }

        protected override void RemoveItem(int index)
        {
            base.RemoveItem(index);
            this.IsTouched = true;
            this.IsDirty = !this.originalItems.SequenceEqual(this);
        }

        protected override void SetItem(int index, T item)
        {
            base.SetItem(index, item);
            this.IsTouched = true;
            this.IsDirty = !this.originalItems.SequenceEqual(this);
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
            this.originalItems = this.ToList();
            this.IsDirty = false;
            this.IsTouched = false;
        }

        public void Reset(ValueResetType resetType = ValueResetType.Hard)
        {
            this.ClearItems();
            if (this.originalItems?.Any() == true)
            {
                this.AddRange(this.originalItems);
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
    }
}
