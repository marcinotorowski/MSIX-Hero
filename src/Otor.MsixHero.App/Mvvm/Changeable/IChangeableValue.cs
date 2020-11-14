using System;

namespace Otor.MsixHero.App.Mvvm.Changeable
{
    public interface IChangeableValue : IChangeable
    {
        /// <summary>
        /// An event fired when the value was changed.
        /// </summary>
        event EventHandler<ValueChangedEventArgs> ValueChanged;

        /// <summary>
        /// An event fired when the value is about to be changed.
        /// </summary>
        event EventHandler<ValueChangingEventArgs> ValueChanging;
    }
}