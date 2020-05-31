using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using otor.msixhero.lib.BusinessLayer.Winget;
using otor.msixhero.lib.Domain.Winget;
using otor.msixhero.lib.Infrastructure;
using otor.msixhero.lib.Infrastructure.Logging;
using otor.msixhero.lib.Infrastructure.Progress;
using otor.msixhero.ui.Commands.RoutedCommand;
using otor.msixhero.ui.Domain;

namespace otor.msixhero.ui.Modules.Dialogs.Winget.ViewModel
{
    public class WingetDefinitionViewModel : ChangeableContainer
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(WingetDefinitionViewModel));

        private readonly IInteractionService interactionService;
        protected readonly YamlWriter YamlWriter = new YamlWriter();
        protected readonly YamlReader YamlReader = new YamlReader();
        protected readonly YamlUtils YamlUtils = new YamlUtils();
        private bool isLoading;
        private YamlDefinition model;
        private bool autoId = true;
        private ICommand loadFromSetup;

        public WingetDefinitionViewModel(IInteractionService interactionService)
        {
            this.interactionService = interactionService;
            this.AddChildren(
                this.Name = new ValidatedChangeableProperty<string>(true, ValidatorFactory.ValidateNotEmptyField("Package name")),
                this.Version = new ValidatedChangeableProperty<string>(true, ValidatorFactory.ValidateVersion(true, "Version")),
                this.Installer = new WingetInstallerViewModel(interactionService),
                this.LicenseUrl = new ValidatedChangeableProperty<string>(true, ValidatorFactory.ValidateUrl(false, "License URL")),
                this.Tags = new ChangeableProperty<string>(),
                this.AppMoniker = new ChangeableProperty<string>(),
                this.Homepage = new ValidatedChangeableProperty<string>(true, ValidatorFactory.ValidateUrl(false, "Homepage URL")),
                this.Id = new ValidatedChangeableProperty<string>(true, ValidateId),
                this.Description = new ChangeableProperty<string>(),
                this.Publisher = new ValidatedChangeableProperty<string>(true, ValidatorFactory.ValidateNotEmptyField("Publisher name")),
                this.License = new ValidatedChangeableProperty<string>(true, ValidatorFactory.ValidateNotEmptyField("License")),
                this.ManifestVersion1 = new ValidatedChangeableProperty<string>(true, ValidatorFactory.ValidateInteger(false, "Manifest version (major)")),
                this.ManifestVersion2 = new ValidatedChangeableProperty<string>(true, ValidatorFactory.ValidateInteger(false, "Manifest version (minor)")),
                this.ManifestVersion3 = new ValidatedChangeableProperty<string>(true, ValidatorFactory.ValidateInteger(false, "Manifest version (revision)")),
                this.MinOSVersion = new ValidatedChangeableProperty<string>(true, ValidatorFactory.ValidateVersion(false, "Minimum OS version")));

            this.Name.ValueChanged += this.NameOnValueChanged;
            this.Publisher.ValueChanged += this.PublisherOnValueChanged;
            this.Id.ValueChanged += this.IdOnValueChanged;

            this.SetValidationMode(ValidationMode.Silent, true);
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
        
        public WingetInstallerViewModel Installer { get; }

        public bool IsLoading
        {
            get => this.isLoading;
            set => this.SetField(ref this.isLoading, value);
        }

        public ICommand LoadFromSetup
        {
            get => this.loadFromSetup ??= new DelegateCommand(this.OnLoadFromSetup);
        }

        public async Task LoadFromYaml(string file, CancellationToken cancellationToken = default)
        {
            try
            {
                this.IsLoading = true;

                using (var fs = File.OpenRead(file))
                {
                    var yaml = await this.YamlReader.ReadAsync(fs, cancellationToken).ConfigureAwait(false);
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
                var yaml = await this.YamlUtils.CreateFromFile(file, cancellationToken).ConfigureAwait(false);
                this.SetData(yaml);
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
        
        private async void OnLoadFromSetup(object obj)
        {
            if (!this.interactionService.SelectFile("MSIX packages|*.msix|MSIX manifests|AppxManifest.xml|Executables|*.exe|Windows Installer|*.msi", out var selected))
            {
                return;
            }

            await this.LoadFromFile(selected, CancellationToken.None).ConfigureAwait(false);
        }

        private void SetData(YamlDefinition definition)
        {
            this.autoId = true;
            this.model = definition;
            
            this.License.CurrentValue = definition.License;
            this.LicenseUrl.CurrentValue = definition.LicenseUrl;
            this.Name.CurrentValue = definition.Name;
            this.Version.CurrentValue = definition.Version;
            this.Publisher.CurrentValue = definition.Publisher;
            this.License.CurrentValue = definition.License;
            this.AppMoniker.CurrentValue = definition.AppMoniker;
            this.Tags.CurrentValue = definition.Tags;
            this.Description.CurrentValue = definition.Description;
            this.Homepage.CurrentValue = definition.Homepage;
            this.MinOSVersion.CurrentValue = definition.MinOperatingSystemVersion?.ToString();
            this.Id.CurrentValue = definition.Id;

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
                this.Installer.SetData(definition.Installers.First());
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
                this.Installer.SetData(newItem);
            }

            this.Commit();
        }

        public async Task<bool> Save(string fileName, CancellationToken cancellationToken, IProgress<ProgressData> progress)
        {
            if (this.ValidationMode == ValidationMode.Silent)
            {
                this.SetValidationMode(ValidationMode.Default, true);
            }

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
            
            this.Installer.Commit();
            
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