using System;

namespace otor.msixhero.ui.Domain
{
    public class ValueChangedEventArgs<T> : EventArgs
    {
        public ValueChangedEventArgs(T newValue)
        {
            this.NewValue = newValue;
        }

        public T NewValue { get; }
    }
}