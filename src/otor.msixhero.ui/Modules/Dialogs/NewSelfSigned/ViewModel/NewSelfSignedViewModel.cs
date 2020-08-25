using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using Otor.MsixHero.Appx.Signing;
using Otor.MsixHero.Infrastructure.Processes.SelfElevation;
using Otor.MsixHero.Infrastructure.Processes.SelfElevation.Enums;
using Otor.MsixHero.Infrastructure.Progress;
using Otor.MsixHero.Infrastructure.Services;
using Otor.MsixHero.Ui.Commands.RoutedCommand;
using Otor.MsixHero.Ui.Controls.ChangeableDialog.ViewModel;
using Otor.MsixHero.Ui.Domain;
using Prism.Services.Dialogs;

namespace Otor.MsixHero.Ui.Modules.Dialogs.NewSelfSigned.ViewModel
{
    public class NewSelfSignedViewModel : ChangeableDialogViewModel
    {
        private readonly ISelfElevationProxyProvider<ISigningManager> signingManagerFactory;
        private bool isSubjectTouched;
        private ICommand importNewCertificate;

        public NewSelfSignedViewModel(
            ISelfElevationProxyProvider<ISigningManager> signingManagerFactory, 
            IInteractionService interactionService, 
            IConfigurationService configurationService) : base("New self signed certificate", interactionService)
        {
            this.signingManagerFactory = signingManagerFactory;
            
            this.OutputPath = new ChangeableFolderProperty(interactionService, configurationService.GetCurrentConfiguration().Signing?.DefaultOutFolder);
            
            this.PublisherName = new ValidatedChangeableProperty<string>("CN=");
            this.PublisherName.ValueChanged += this.PublisherNameOnValueChanged;
            this.PublisherName.Validators = new Func<string, string>[] { ValidatePublisherName };
            
            this.PublisherFriendlyName = new ValidatedChangeableProperty<string>();
            this.PublisherFriendlyName.ValueChanged += this.PublisherFriendlyNameOnValueChanged;
            this.PublisherFriendlyName.Validators = new Func<string, string>[] { ValidatePublisherFriendlyName };

            this.Password = new ValidatedChangeableProperty<string>();
            this.Password.Validators = new Func<string, string>[] { ValidatePassword };
            
            this.AddChildren(this.OutputPath, this.Password, this.PublisherName, this.PublisherFriendlyName);
            this.SetValidationMode(ValidationMode.Silent, true);
        }
        
        public ValidatedChangeableProperty<string> PublisherName { get; }
        
        public ValidatedChangeableProperty<string> PublisherFriendlyName { get; }

        public ValidatedChangeableProperty<string> Password { get; }

        public ChangeableFolderProperty OutputPath { get; }

        public ICommand ImportNewCertificate
        {
            get => this.importNewCertificate ??= new DelegateCommand(param => this.ImportNewCertificateExecute());
        }

        protected override async Task<bool> Save(CancellationToken cancellationToken, IProgress<ProgressData> progress)
        {
            var manager = await this.signingManagerFactory.GetProxyFor(SelfElevationLevel.AsInvoker, cancellationToken).ConfigureAwait(false);
            cancellationToken.ThrowIfCancellationRequested();

            await manager.CreateSelfSignedCertificate(
                new DirectoryInfo(this.OutputPath.CurrentValue),
                this.PublisherName.CurrentValue,
                this.PublisherFriendlyName.CurrentValue,
                this.Password.CurrentValue,
                cancellationToken,
                progress);

            return true;
        }

        private static string ValidatePublisherName(string newValue)
        {
            if (string.IsNullOrEmpty(newValue))
            {
                return "The display name of the publisher may not be empty.";
            }
            
            if (!Regex.IsMatch(newValue, "^[a-zA-Z]+=.+"))
            {
                // todo: Some better validation, RFC compliant
                return "Publisher name must be a valid DN string (for example CN=Author)";
            }

            return null;
        }

        private static string ValidatePublisherFriendlyName(string newValue)
        {
            return string.IsNullOrEmpty(newValue) ? "The name of the publisher may not be empty." : null;
        }

        private static string ValidatePassword(string currentValue)
        {
            return string.IsNullOrEmpty(currentValue) ? "The password may not be empty." : null;
        }

        private async void ImportNewCertificateExecute()
        {
            var file = Directory.EnumerateFiles(this.OutputPath.CurrentValue, "*.cer").OrderByDescending(d => new FileInfo(d).LastWriteTimeUtc).FirstOrDefault();
            if (file == null)
            {
                return;
            }

            var mgr = await this.signingManagerFactory.GetProxyFor(SelfElevationLevel.HighestAvailable).ConfigureAwait(false);
            await mgr.InstallCertificate(file, CancellationToken.None).ConfigureAwait(false);

            this.CloseCommand.Execute(ButtonResult.OK);
        }
        
        private void PublisherNameOnValueChanged(object sender, ValueChangedEventArgs e)
        {
            this.isSubjectTouched = true;
        }

        private void PublisherFriendlyNameOnValueChanged(object sender, ValueChangedEventArgs e)
        {
            if (this.isSubjectTouched)
            {
                return;
            }

            var touch = this.isSubjectTouched;
            try
            {
                this.PublisherName.CurrentValue = "CN=" + (string) e.NewValue;
            }
            finally
            {
                this.isSubjectTouched = touch;
            }
        }
    }
}

