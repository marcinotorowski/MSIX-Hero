// MSIX Hero
// Copyright (C) 2021 Marcin Otorowski
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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using Otor.MsixHero.App.Helpers;
using Otor.MsixHero.App.Mvvm.Changeable;
using Otor.MsixHero.Appx.Packaging;
using Otor.MsixHero.Infrastructure.Logging;
using Otor.MsixHero.Infrastructure.Progress;
using Otor.MsixHero.Infrastructure.Services;
using Otor.MsixHero.Winget.Yaml;
using Otor.MsixHero.Winget.Yaml.Entities;
using Prism.Commands;

namespace Otor.MsixHero.App.Modules.Dialogs.WinGet.YamlEditor.ViewModel
{
    public class WingetDefinitionViewModel : ChangeableContainer
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(WingetDefinitionViewModel));

        private readonly IInteractionService interactionService;
        protected readonly YamlWriter YamlWriter = new YamlWriter();
        protected readonly YamlReader YamlReader = new YamlReader();
        protected readonly YamlUtils YamlUtils = new YamlUtils();
        private bool isLoading;
        private YamlDefinition model = new YamlDefinition();
        private bool autoId = true;
        private ICommand loadFromSetup;
        private ICommand generateSha256, openSha256;

        public WingetDefinitionViewModel(IInteractionService interactionService)
        {
            this.interactionService = interactionService;

            this.Name = new ValidatedChangeableProperty<string>("Package name", true, ValidatorFactory.ValidateNotEmptyField());
            this.Publisher = new ValidatedChangeableProperty<string>("Package publisher", true, ValidatorFactory.ValidateNotEmptyField());
            this.Version = new ValidatedChangeableProperty<string>("Version", true, ValidatorFactory.ValidateVersion(true));
            this.Id = new ValidatedChangeableProperty<string>("Package identifier", true, ValidateId);
            this.ManifestVersion1 = new ValidatedChangeableProperty<string>("Manifest version", true, ValidatorFactory.ValidateInteger(false, "Major version"));
            this.ManifestVersion2 = new ValidatedChangeableProperty<string>("Manifest version", true, ValidatorFactory.ValidateInteger(false, "Minor version"));
            this.ManifestVersion3 = new ValidatedChangeableProperty<string>("Manifest version", true, ValidatorFactory.ValidateInteger(false, "Revision"));
            this.AppMoniker = new ChangeableProperty<string>();
            this.Tags = new ChangeableProperty<string>();
            this.Homepage = new ValidatedChangeableProperty<string>("Home page", true, ValidatorFactory.ValidateUrl(false));
            this.Description = new ChangeableProperty<string>();
            this.MinOSVersion = new ValidatedChangeableProperty<string>("Minimum OS version", true, ValidatorFactory.ValidateVersion(false));
            this.Url = new ValidatedChangeableProperty<string>("Installer URL", ValidatorFactory.ValidateUrl(true));
            this.Sha256 = new ValidatedChangeableProperty<string>("Installer hash", ValidatorFactory.ValidateSha256(true));
            this.LicenseUrl = new ValidatedChangeableProperty<string>("License URL", true, ValidatorFactory.ValidateUrl(false));
            this.License = new ValidatedChangeableProperty<string>("License", true, ValidatorFactory.ValidateNotEmptyField());
            this.TabIdentity = new ChangeableContainer(this.Name, this.Publisher, this.Version, this.Id, this.ManifestVersion1, this.ManifestVersion2, this.ManifestVersion3);
            this.TabMetadata = new ChangeableContainer(this.AppMoniker, this.Tags, this.Homepage, this.Description, this.MinOSVersion);
            this.TabDownloads = new ChangeableContainer(this.Url, this.Sha256);
            this.TabInstaller = new WingetInstallerViewModel(this.YamlUtils, this.interactionService) { Url = this.Url.CurrentValue };
            this.TabLicense = new ChangeableContainer(this.License, this.LicenseUrl);

            this.AddChildren(
                this.TabIdentity,
                this.TabMetadata,
                this.TabDownloads,
                this.TabInstaller,
                this.TabLicense);

            this.Name.ValueChanged += this.NameOnValueChanged;
            this.Publisher.ValueChanged += this.PublisherOnValueChanged;
            this.Id.ValueChanged += this.IdOnValueChanged;
            this.Url.ValueChanged += this.UrlOnValueChanged;
        }

        public ChangeableContainer TabIdentity { get; }

        public ChangeableContainer TabMetadata { get; }

        public ChangeableContainer TabDownloads { get; }

        public ChangeableContainer TabLicense { get; }

        public ChangeableProperty<string> Url { get; }

        public ChangeableProperty<string> Sha256 { get; }

        public ProgressProperty HashingProgress { get; } = new ProgressProperty();

        private bool isGenerateHashShown;
        public bool IsGenerateHashShown
        {
            get => this.isGenerateHashShown;
            set => this.SetField(ref this.isGenerateHashShown, value);
        }

        public static string ValidateId(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return "Application ID may not be empty.";
            }

            if (id.IndexOf('.') == -1)
            {
                return "Application ID should contain a dot to separate the publisher from the product name.";
            }

            return null;
        }

        public ValidatedChangeableProperty<string> Name { get; }

        public ChangeableProperty<string> AppMoniker { get; }

        public ValidatedChangeableProperty<string> Publisher { get; }

        public ValidatedChangeableProperty<string> Id { get; }

        public ValidatedChangeableProperty<string> ManifestVersion1 { get; }
        
        public ValidatedChangeableProperty<string> ManifestVersion2 { get; }

        public ValidatedChangeableProperty<string> ManifestVersion3 { get; }

        public bool ShowManifestVersion { get; private set; }

        public ValidatedChangeableProperty<string> Version { get; }
        
        // ReSharper disable once InconsistentNaming
        public ValidatedChangeableProperty<string> MinOSVersion { get; }

        public ValidatedChangeableProperty<string> License { get; }

        public ChangeableProperty<string> Tags { get; }

        public ValidatedChangeableProperty<string> Homepage { get; }

        public ChangeableProperty<string> Description { get; }

        public ValidatedChangeableProperty<string> LicenseUrl { get; }
        
        public WingetInstallerViewModel TabInstaller { get; }

        public bool IsLoading
        {
            get => this.isLoading;
            set => this.SetField(ref this.isLoading, value);
        }

        public ICommand LoadFromSetup => this.loadFromSetup ??= new DelegateCommand(this.OnLoadFromSetup);

        public async Task LoadFromYaml(string file, CancellationToken cancellationToken = default)
        {
            try
            {
                this.IsLoading = true;

                using (var fs = File.OpenRead(file))
                {
                    var yaml = await this.YamlReader.ReadAsync(fs, cancellationToken).ConfigureAwait(true);
                    this.YamlUtils.FillGaps(yaml);
                    this.SetData(yaml);
                }
            }
            finally
            {
                this.IsLoading = false;
            }
        }

        public async Task LoadFromFile(string file, CancellationToken cancellationToken = default)
        {
            try
            {
                this.IsLoading = true;
                var yaml = await this.YamlUtils.CreateFromFile(file, cancellationToken).ConfigureAwait(true);
                this.SetData(yaml, false);
            }
            catch (Exception e)
            {
                Logger.Error(e);
                this.interactionService.ShowError("Could not load the details from the selected file.", e);
            }
            finally
            {
                this.IsLoading = false;
            }
        }
        
        public Task NewManifest(CancellationToken cancellationToken)
        {
            var newItem = new YamlDefinition();

            this.SetData(newItem);

            return Task.FromResult(true);
        }

        private void NameOnValueChanged(object sender, ValueChangedEventArgs e)
        {
            if (!this.autoId)
            {
                return;
            }

            this.Id.CurrentValue = $"{this.Publisher.CurrentValue}.{this.Name.CurrentValue}".Replace(" ", string.Empty);
            this.autoId = true;
        }

        private void PublisherOnValueChanged(object sender, ValueChangedEventArgs e)
        {
            if (!this.autoId)
            {
                return;
            }

            this.Id.CurrentValue = $"{this.Publisher.CurrentValue}.{this.Name.CurrentValue}".Replace(" ", string.Empty);
            this.autoId = true;
        }

        private void IdOnValueChanged(object sender, ValueChangedEventArgs e)
        {
            Logger.Debug($"Package ID is not touched manually and will not auto update...");
            this.autoId = false;
        }
        private void UrlOnValueChanged(object sender, ValueChangedEventArgs e)
        {
            this.TabInstaller.Url = (string) e.NewValue;
        }
        
        private async void OnLoadFromSetup()
        {
            var settings = FileDialogSettings.FromFilterString(new DialogFilterBuilder("*" + FileConstants.MsixExtension, "*" + FileConstants.AppxExtension, FileConstants.AppxManifestFile, "*.exe", "*.msi").BuildFilter());
            // ReSharper disable once StringLiteralTypo
            if (!this.interactionService.SelectFile(settings, out var selected))
            {
                return;
            }

            await this.LoadFromFile(selected, CancellationToken.None).ConfigureAwait(false);
        }

        public ICommand GenerateSha256
        {
            get
            {
                return this.generateSha256 ??= new DelegateCommand<string>(this.GenerateHash);
            }
        }
        
        public ICommand OpenSha256
        {
            get
            {
                return this.openSha256 ??= new DelegateCommand<string>(this.OpenHash);
            }
        }

        private async void GenerateHash(string parameter)
        {
            if (string.IsNullOrEmpty(this.Url.CurrentValue))
            {
                this.interactionService.ShowError("You must first configure the installer URL before a hash can be calculated.");
                return;
            }

            if (this.interactionService.Confirm($"This will download the file '{this.Url.CurrentValue}' and calculate its hash. The download may take a while, do you want to continue?", type: InteractionType.Question, buttons: InteractionButton.YesNo) == InteractionResult.No)
            {
                return;
            }

            var progress = new Progress();
            try
            {
                using (var cts = new CancellationTokenSource())
                {
                    var task = this.YamlUtils.CalculateHashAsync(new Uri(this.Url.CurrentValue), cts.Token, progress);
                    this.HashingProgress.MonitorProgress(task, cts, progress);
                    var newHash = await task.ConfigureAwait(true);

                    // this is to make sure that the hash is uppercase or lowercase depending on the source. We prefer lowercase
                    if (true == this.Sha256.CurrentValue?.All(c => char.IsUpper(c) || char.IsDigit(c)))
                    {
                        newHash = newHash.ToUpperInvariant();
                    }
                    else
                    {
                        newHash = newHash.ToLowerInvariant();
                    }

                    this.Sha256.CurrentValue = newHash;
                }
            }
            catch (Exception e)
            {
                this.interactionService.ShowError(e.Message, e);
            }
        }

        private async void OpenHash(string parameter)
        {
            if (!this.interactionService.SelectFile(out var path))
            {
                return;
            }

            var progress = new Progress();
            using (var cts = new CancellationTokenSource())
            {
                try
                {
                    var task2 = this.YamlUtils.CalculateHashAsync(new FileInfo(path), cts.Token, progress);
                    this.HashingProgress.MonitorProgress(task2, cts, progress);
                    this.Sha256.CurrentValue = await task2.ConfigureAwait(true);
                }
                catch (Exception e)
                {
                    this.interactionService.ShowError($"The file could not be hashed. {e.Message}", e);
                }
            }
        }

        private void SetData(YamlDefinition definition, bool useNullValues = true)
        {
            this.autoId = true;
            this.model = definition;

            if (useNullValues || !string.IsNullOrEmpty(definition.License))
            {
                this.License.CurrentValue = definition.License;
            }

            if (useNullValues || !string.IsNullOrEmpty(definition.LicenseUrl))
            {
                this.LicenseUrl.CurrentValue = definition.LicenseUrl;
            }

            if (useNullValues || !string.IsNullOrEmpty(definition.Name))
            {
                this.Name.CurrentValue = definition.Name;
            }
            
            if (useNullValues || !string.IsNullOrEmpty(definition.Version))
            {
                this.Version.CurrentValue = definition.Version;
            }

            if (useNullValues || !string.IsNullOrEmpty(definition.Publisher))
            {
                this.Publisher.CurrentValue = definition.Publisher;
            }

            if (useNullValues || !string.IsNullOrEmpty(definition.License))
            {
                this.License.CurrentValue = definition.License;
            }

            if (useNullValues || !string.IsNullOrEmpty(definition.AppMoniker))
            {
                this.AppMoniker.CurrentValue = definition.AppMoniker;
            }

            if (useNullValues || !string.IsNullOrEmpty(definition.Tags))
            {
                this.Tags.CurrentValue = definition.Tags;
            }

            if (useNullValues || !string.IsNullOrEmpty(definition.Description))
            {
                this.Description.CurrentValue = definition.Description;
            }

            if (useNullValues || !string.IsNullOrEmpty(definition.Homepage))
            {
                this.Homepage.CurrentValue = definition.Homepage;
            }

            if (useNullValues || definition.MinOperatingSystemVersion != default)
            {
                this.MinOSVersion.CurrentValue = definition.MinOperatingSystemVersion?.ToString();
            }

            if (useNullValues || !string.IsNullOrEmpty(definition.Id))
            {
                this.Id.CurrentValue = definition.Id;
            }

            this.ShowManifestVersion = definition.ManifestVersion != default;
            this.OnPropertyChanged(nameof(ShowManifestVersion));

            if (definition.ManifestVersion != null)
            {
                this.ManifestVersion1.CurrentValue = definition.ManifestVersion.Major.ToString("0");
                this.ManifestVersion2.CurrentValue = definition.ManifestVersion.Minor.ToString("0");
                this.ManifestVersion3.CurrentValue = definition.ManifestVersion.Build.ToString("0");
            }

            if (definition.Installers?.Any() == true)
            {
                var installer = definition.Installers.First();
                this.TabInstaller.SetData(installer, useNullValues);

                if (useNullValues || installer.Url != null)
                {
                    this.Url.CurrentValue = installer?.Url;
                }

                if (useNullValues || installer.Sha256 != null)
                {
                    this.Sha256.CurrentValue = installer?.Sha256;
                }
            }
            else
            {
                var newItem = new YamlInstaller
                {
                    InstallerType = YamlInstallerType.msix,
                    Arch = YamlArchitecture.x64,
                    Scope = YamlScope.machine
                };
                
                definition.Installers = new List<YamlInstaller> { newItem };
                this.TabInstaller.SetData(newItem);
            }

            this.Commit();
        }

        public async Task<bool> Save(string fileName, CancellationToken cancellationToken, IProgress<ProgressData> progress)
        {
            if (!this.IsValid)
            {
                return false;
            }

            this.model.Name = this.Name.CurrentValue;
            this.model.AppMoniker = this.AppMoniker.CurrentValue;
            this.model.Description = this.Description.CurrentValue;
            this.model.License = this.License.CurrentValue;
            this.model.Homepage = this.Homepage.CurrentValue;
            this.model.Id = this.Id.CurrentValue;
            this.model.Publisher = this.Publisher.CurrentValue;
            this.model.MinOperatingSystemVersion = string.IsNullOrEmpty(this.MinOSVersion.CurrentValue) ? null : System.Version.Parse(this.MinOSVersion.CurrentValue);
            this.model.Tags = this.Tags.CurrentValue;
            this.model.Version = this.Version.CurrentValue;
            this.model.LicenseUrl = this.LicenseUrl.CurrentValue;

            if (!string.IsNullOrEmpty(this.ManifestVersion1.CurrentValue) || !string.IsNullOrEmpty(this.ManifestVersion2.CurrentValue) || !string.IsNullOrEmpty(this.ManifestVersion3.CurrentValue))
            {
                var v1 = this.ManifestVersion1.CurrentValue;
                var v2 = this.ManifestVersion2.CurrentValue;
                var v3 = this.ManifestVersion3.CurrentValue;

                if (string.IsNullOrWhiteSpace(v1))
                {
                    v1 = "0";
                }

                if (string.IsNullOrWhiteSpace(v2))
                {
                    v2 = "0";
                }

                if (string.IsNullOrWhiteSpace(v3))
                {
                    v3 = "0";
                }

                this.model.ManifestVersion = System.Version.Parse($"{v1}.{v2}.{v3}");
                Logger.Info("Manifest version changed to {0}.", (object) this.model.ManifestVersion);
            }
            
            this.TabInstaller.Commit();
            this.model.Installers[0].Url = this.Url.CurrentValue;
            this.model.Installers[0].Sha256 = this.Sha256.CurrentValue;
            
            var fileInfo = new FileInfo(fileName);
            if (fileInfo.Directory != null && !fileInfo.Directory.Exists)
            {
                fileInfo.Directory.Create();
            }

            using (var fs = File.OpenWrite(fileName))
            {
                Logger.Info("Writing YAML manifest to {0}", (object) fileName);
                await this.YamlWriter.WriteAsync(model, fs, cancellationToken).ConfigureAwait(false);
            }

            return true;
        }
    }
}