using System;

namespace Otor.MsixHero.App.Changeable
{
    public abstract class ValueChangedEventArgsBase : EventArgs
    {
        public ValueChangedEventArgsBase(object oldValue, object newValue, object originalValue, bool wasDirty, bool wasTouched)
        {
            this.OldValue = oldValue;
            this.NewValue = newValue;
            this.OriginalValue = originalValue;
            this.WasDirty = wasDirty;
            this.WasTouched = wasTouched;
        }

        public object OldValue { get; private set; }

        public object NewValue { get; private set; }

        public object OriginalValue { get; private set; }

        public bool WasDirty { get; private set; }

        public bool WasTouched { get; private set; }
    }
}