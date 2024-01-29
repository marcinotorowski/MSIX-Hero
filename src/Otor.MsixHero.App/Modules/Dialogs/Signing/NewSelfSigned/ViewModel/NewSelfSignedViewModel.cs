// MSIX Hero
// Copyright (C) 2024 Marcin Otorowski
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// Full notice:
// https://github.com/marcinotorowski/msix-hero/blob/develop/LICENSE.md

using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using Otor.MsixHero.App.Mvvm.Changeable;
using Otor.MsixHero.App.Mvvm.Changeable.Dialog.ViewModel;
using Otor.MsixHero.Appx.Editor;
using Otor.MsixHero.Appx.Signing;
using Otor.MsixHero.Cli.Verbs;
using Otor.MsixHero.Infrastructure.Progress;
using Otor.MsixHero.Infrastructure.Services;
using Prism.Commands;
using Prism.Services.Dialogs;

namespace Otor.MsixHero.App.Modules.Dialogs.Signing.NewSelfSigned.ViewModel
{
    public class NewSelfSignedViewModel : ChangeableAutomatedDialogViewModel<NewCertVerb>
    {
        private readonly ISigningManager signingManagerFactory;
        private bool isSubjectTouched;
        private ICommand importNewCertificate;

        public NewSelfSignedViewModel(
            ISigningManager signingManagerFactory, 
            IInteractionService interactionService, 
            IConfigurationService configurationService) : base(Resources.Localization.Dialogs_NewCert_Title, interactionService)
        {
            this.signingManagerFactory = signingManagerFactory;
            
            this.OutputPath = new ChangeableFolderProperty(() => Resources.Localization.Dialogs_NewCert_OutputPath, interactionService, configurationService.GetCurrentConfiguration().Signing?.DefaultOutFolder);
            this.PublisherName = new ValidatedChangeableProperty<string>(() => Resources.Localization.Dialogs_NewCert_PublisherName, "CN=", AppxValidatorFactory.ValidateSubject());
            this.PublisherFriendlyName = new ValidatedChangeableProperty<string>(() => Resources.Localization.Dialogs_NewCert_PublisherDisplayName, ValidatePublisherFriendlyName);
            this.Password = new ValidatedChangeableProperty<string>(() => Resources.Localization.Dialogs_NewCert_Password, ValidatePassword);
            this.ValidUntil = new ValidatedChangeableProperty<DateTime>(() => Resources.Localization.Dialogs_NewCert_ValidUntil, DateTime.Now.Add(TimeSpan.FromDays(365)), ValidateDateTime);
            this.AddChildren(this.PublisherFriendlyName, this.PublisherName, this.ValidUntil, this.Password, this.OutputPath);

            this.PublisherName.ValueChanged += this.PublisherNameOnValueChanged;
            this.PublisherFriendlyName.ValueChanged += this.PublisherFriendlyNameOnValueChanged;

            this.RegisterForCommandLineGeneration(this.PublisherFriendlyName, this.PublisherName, this.ValidUntil, this.Password, this.OutputPath);
        }

        protected override void UpdateVerbData()
        {
            this.Verb.ValidUntil = this.ValidUntil.CurrentValue;
            this.Verb.Password = this.Password.CurrentValue;
            this.Verb.DisplayName = this.PublisherFriendlyName.CurrentValue;
            this.Verb.Subject = this.PublisherName.CurrentValue;
            this.Verb.OutputFolder = this.OutputPath.CurrentValue;

            if (string.IsNullOrEmpty(this.Verb.DisplayName))
            {
                this.Verb.DisplayName = Resources.Localization.Dialogs_NewCert_Placeholder_DisplayName;
            }

            if (string.IsNullOrEmpty(this.Verb.Subject))
            {
                this.Verb.DisplayName = Resources.Localization.Dialogs_NewCert_Placeholder_Name;
            }

            if (string.IsNullOrEmpty(this.Verb.OutputFolder))
            {
                this.Verb.DisplayName = Resources.Localization.Dialogs_NewCert_Placeholder_OutputDir;
            }
        }

        public ValidatedChangeableProperty<string> PublisherName { get; }
        
        public ValidatedChangeableProperty<string> PublisherFriendlyName { get; }
        
        public ValidatedChangeableProperty<DateTime> ValidUntil { get; }

        public ValidatedChangeableProperty<string> Password { get; }

        public ChangeableFolderProperty OutputPath { get; }

        public ICommand ImportNewCertificate => this.importNewCertificate ??= new DelegateCommand(this.ImportNewCertificateExecute);

        protected override async Task<bool> Save(CancellationToken cancellationToken, IProgress<ProgressData> progress)
        {
            cancellationToken.ThrowIfCancellationRequested();

            await this.signingManagerFactory.CreateSelfSignedCertificate(
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
            return string.IsNullOrEmpty(newValue) ? Resources.Localization.Dialogs_NewCert_Validation_PublisherDisplayName : null;
        }

        private static string ValidateDateTime(DateTime date)
        {
            return date > DateTime.Now ? null : Resources.Localization.Dialogs_NewCert_Validation_DatePast;
        }

        private static string ValidatePassword(string currentValue)
        {
            return string.IsNullOrEmpty(currentValue) ? Resources.Localization.Dialogs_NewCert_Validation_EmptyPwd : null;
        }

        private async void ImportNewCertificateExecute()
        {
            var file = Directory.EnumerateFiles(this.OutputPath.CurrentValue, "*.cer").OrderByDescending(d => new FileInfo(d).LastWriteTimeUtc).FirstOrDefault();
            if (file == null)
            {
                return;
            }

            var mgr = this.signingManagerFactory;
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

