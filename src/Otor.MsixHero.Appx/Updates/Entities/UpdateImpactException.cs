using System;

namespace Otor.MsixHero.Appx.Updates.Entities
{
    public class UpdateImpactException : Exception
    {
        public UpdateImpactException() : this("Selected packages are not upgradable.", UpgradeImpactError.Unknown)
        {
        }

        public UpdateImpactException(UpgradeImpactError error) : this("Selected packages are not upgradable.", error)
        {
        }

        public UpdateImpactException(string message) : base(message)
        {
            this.ErrorType = UpgradeImpactError.Unknown;
        }

        public UpdateImpactException(string message, UpgradeImpactError error) : base(message)
        {
            this.ErrorType = error;
        }

        public UpdateImpactException(string message, Exception innerException) : base(message, innerException)
        {
            this.ErrorType = UpgradeImpactError.Unknown;
        }

        public UpdateImpactException(string message, UpgradeImpactError error, Exception innerException) : base(message, innerException)
        {
            this.ErrorType = error;
        }

        public UpdateImpactException(UpgradeImpactError error, Exception innerException) : base("Selected packages are not upgradable.", innerException)
        {
            this.ErrorType = error;
        }

        public UpgradeImpactError ErrorType { get; private set; }
    }
}
