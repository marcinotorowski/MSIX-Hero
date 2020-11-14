using System;

namespace Otor.MsixHero.App.Mvvm.Changeable
{
    public interface IValidatedChangeable : IChangeable
    {
        string ValidationMessage { get; }

        bool IsValidated { get; set; }

        bool IsValid { get; }

        bool DisplayValidationErrors { get; set; }


        event EventHandler<ValueChangedEventArgs<string>> ValidationStatusChanged;
    }
}