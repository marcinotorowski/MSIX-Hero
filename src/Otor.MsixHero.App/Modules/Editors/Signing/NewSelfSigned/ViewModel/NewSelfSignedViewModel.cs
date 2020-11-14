using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using Otor.MsixHero.App.Mvvm.Changeable;
using Otor.MsixHero.App.Mvvm.Changeable.Dialog.ViewModel;
using Otor.MsixHero.Appx.Signing;
using Otor.MsixHero.Infrastructure.Processes.SelfElevation;
using Otor.MsixHero.Infrastructure.Processes.SelfElevation.Enums;
using Otor.MsixHero.Infrastructure.Progress;
using Otor.MsixHero.Infrastructure.Services;
using Prism.Commands;
using Prism.Services.Dialogs;

namespace Otor.MsixHero.App.Modules.Editors.Signing.NewSelfSigned.ViewModel
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
            
            this.OutputPath = new ChangeableFolderProperty("Output path", interactionService, configurationService.GetCurrentConfiguration().Signing?.DefaultOutFolder);
            this.PublisherName = new ValidatedChangeableProperty<string>("Publisher name", "CN=", ValidatorFactory.ValidateSubject());
            this.PublisherFriendlyName = new ValidatedChangeableProperty<string>("Publisher display name", ValidatePublisherFriendlyName);
            this.Password = new ValidatedChangeableProperty<string>("Password", ValidatePassword);
            this.ValidUntil = new ValidatedChangeableProperty<DateTime>("Valid until", DateTime.Now.Add(TimeSpan.FromDays(365)), ValidateDateTime);
            this.AddChildren(this.PublisherFriendlyName, this.PublisherName, this.ValidUntil, this.Password, this.OutputPath);

            this.PublisherName.ValueChanged += this.PublisherNameOnValueChanged;
            this.PublisherFriendlyName.ValueChanged += this.PublisherFriendlyNameOnValueChanged;
        }
        
        public ValidatedChangeableProperty<string> PublisherName { get; }
        
        public ValidatedChangeableProperty<string> PublisherFriendlyName { get; }
        
        public ValidatedChangeableProperty<DateTime> ValidUntil { get; }

        public ValidatedChangeableProperty<string> Password { get; }

        public ChangeableFolderProperty OutputPath { get; }

        public ICommand ImportNewCertificate => this.importNewCertificate ??= new DelegateCommand(this.ImportNewCertificateExecute);

        protected override async Task<bool> Save(CancellationToken cancellationToken, IProgress<ProgressData> progress)
        {
            var manager = await this.signingManagerFactory.GetProxyFor(SelfElevationLevel.AsInvoker, cancellationToken).ConfigureAwait(false);
            cancellationToken.ThrowIfCancellationRequested();

            await manager.CreateSelfSignedCertificate(
                new DirectoryInfo(this.OutputPath.CurrentValue),
                this.PublisherName.CurrentValue,
                this.PublisherFriendlyName.CurrentValue,
                this.Password.CurrentValue,
                this.ValidUntil.CurrentValue,
                cancellationToken,
                progress).ConfigureAwait(false);

            return true;
        }

        private static string ValidatePublisherFriendlyName(string newValue)
        {
            return string.IsNullOrEmpty(newValue) ? "The name of the publisher may not be empty." : null;
        }

        private static string ValidateDateTime(DateTime date)
        {
            return date > DateTime.Now ? null : "The date lies in the past.";
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

            var mgr = await this.signingManagerFactory.GetProxyFor(SelfElevationLevel.AsAdministrator).ConfigureAwait(true);
            await mgr.InstallCertificate(file, CancellationToken.None).ConfigureAwait(true);

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

