namespace Otor.MsixHero.App.Changeable
{
    public class ValueChangedEventArgs : ValueChangedEventArgsBase
    {
        public ValueChangedEventArgs(object oldValue, object newValue, object originalValue, bool wasDirty, bool wasTouched) : base(oldValue, newValue, originalValue, wasDirty, wasTouched)
        {
        }
    }
}