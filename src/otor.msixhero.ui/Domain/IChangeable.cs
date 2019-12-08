using System;
using System.Collections.Generic;

namespace otor.msixhero.ui.Domain
{
    public interface IChangeable
    {
        /// <summary>
        /// Gets a value indicating if this instance is "dirty", which means that its current value is different from the original one.
        /// </summary>
        bool IsDirty { get; }

        /// <summary>
        /// Gets a value indicating if this instance is "touched", which means that it has been changed even though the current value may be the same as te original one.
        /// </summary>
        bool IsTouched { get; }

        /// <summary>
        /// Commits the current value to the original one, thus promoting it. It resets the "dirty" and "touched" flag.
        /// </summary>
        void Commit();

        /// <summary>
        /// Resets this instance.
        /// </summary>
        /// <param name="resetType">The type of reset determining whether to reset the "touched" flag (hard reset) or not ("soft reset").</param>
        void Reset(ValueResetType resetType = ValueResetType.Hard);

        /// <summary>
        /// Touches this instance. Even if the value is equal to the original one, the property will be marked as "touched".
        /// </summary>
        void Touch();

        event EventHandler<ValueChangedEventArgs<bool>> IsDirtyChanged;

        event EventHandler<ValueChangedEventArgs<bool>> IsTouchedChanged;
    }

    public interface IChangeable<T> : IChangeableValue
    {
        /// <summary>
        /// Gets the original value.
        /// </summary>
        T OriginalValue { get; }

        /// <summary>
        /// Gets or sets the current value.
        /// </summary>
        T CurrentValue { get; set; }
    }

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


    public interface IValidatedChangeable : IChangeable
    {
        string ValidationMessage { get; }

        bool IsValidated { get; set; }

        bool IsValid { get; }

        event EventHandler<ValueChangedEventArgs<string>> ValidationStatusChanged;
    }


    public interface IValidatedChangeable<T> : IChangeable<T>, IValidatedChangeable
    {
        IReadOnlyCollection<Func<T, string>> Validators { get; set; }
    }
}