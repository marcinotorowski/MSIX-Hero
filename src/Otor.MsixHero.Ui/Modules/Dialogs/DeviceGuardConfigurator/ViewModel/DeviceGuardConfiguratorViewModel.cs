using System.Security;
using Otor.MsixHero.Ui.Controls.ChangeableDialog;
using Otor.MsixHero.Ui.Domain;

namespace Otor.MsixHero.Ui.Modules.Dialogs.DeviceGuardConfigurator.ViewModel
{
    public class DeviceGuardConfiguratorViewModel : ChangeableDialog
    {
        public DeviceGuardConfiguratorViewModel()
        {
            this.ClientId = new ValidatedChangeableProperty<string>("Client ID", ValidatorFactory.ValidateGuid(true));
            this.ClientSecret = new ValidatedChangeableProperty<SecureString>("Client secret", this.ValidateSecret);
            this.CertificateSubject = new ValidatedChangeableProperty<string>("Certificate subject", ValidatorFactory.ValidateSubject(false));
        }

        public ValidatedChangeableProperty<string> ClientId { get; }

        public ValidatedChangeableProperty<SecureString> ClientSecret { get; }

        public ValidatedChangeableProperty<string> CertificateSubject { get; }

        private string ValidateSecret(SecureString secret)
        {
            return secret == null || secret.Length == 0 ? "The value is required." : null;
        }
    }
}
