using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using otor.msixhero.lib.BusinessLayer.Appx.Manifest;
using otor.msixhero.lib.BusinessLayer.Appx.Manifest.FileReaders;
using otor.msixhero.lib.BusinessLayer.Winget;
using otor.msixhero.lib.Domain.Appx.Manifest.Full;
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
        private ICommand loadIdentityFromMsix;

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

        public ICommand LoadIdentityFromMsix
        {
            get => this.loadIdentityFromMsix ??= new DelegateCommand(this.OnLoadIdentityFromMsix);
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
        
        public async Task LoadFromMsix(string file, CancellationToken cancellationToken = default)
        {
            try
            {
                this.IsLoading = true;
                
                if (string.Equals(Path.GetExtension(file), ".xml", StringComparison.OrdinalIgnoreCase))
                {
                    using (IAppxFileReader src = new FileInfoFileReaderAdapter(file))
                    {
                        var appxManifestReader = new AppxManifestReader();
                        var manifest = await appxManifestReader.Read(src, cancellationToken).ConfigureAwait(false);

                        var hashSignature = await this.YamlUtils.CalculateSignatureHashAsync(new FileInfo(file), cancellationToken).ConfigureAwait(false);
                        await this.LoadFromMsix(manifest, null, hashSignature).ConfigureAwait(false);
                    }
                }
                else
                {
                    using (IAppxFileReader src = new ZipArchiveFileReaderAdapter(file))
                    {
                        var appxManifestReader = new AppxManifestReader();
                        var manifest = await appxManifestReader.Read(src, cancellationToken).ConfigureAwait(false);

                        var hash = await this.YamlUtils.CalculateHashAsync(new FileInfo(file), cancellationToken).ConfigureAwait(false);

                        if (src.FileExists("AppxSignature.p7x"))
                        {
                            using (var appxSignature = src.GetFile("AppxSignature.p7x"))
                            {
                                string sha256Signature = null;

                                if (appxSignature != null)
                                {
                                    using (var sha256 = SHA256.Create())
                                    {
                                        var byteHash = sha256.ComputeHash(appxSignature);

                                        var builder = new StringBuilder();
                                        foreach (var b in byteHash)
                                        {
                                            cancellationToken.ThrowIfCancellationRequested();
                                            builder.Append(b.ToString("X2"));
                                        }

                                        sha256Signature = builder.ToString();
                                    }
                                }

                                await this.LoadFromMsix(manifest, hash, sha256Signature).ConfigureAwait(false);
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                var fileName = Path.GetFileName(file);
                this.interactionService.ShowError($"MSIX file '{fileName}' could not be opened. {e.Message}", e);
            }
            finally
            {
                this.IsLoading = false;
            }
        }

        private Task LoadFromMsix(AppxPackage package, string hashSha256 = null, string hashSha256Signature = null)
        {
            var yaml = new YamlDefinition
            {
                Publisher = package.PublisherDisplayName,
                Name = package.DisplayName,
                Id = package.Publisher + "." + package.Name,
                // Author = package.PublisherDisplayName,
                MinOperatingSystemVersion = System.Version.Parse("10.0.0"),
                Version = System.Version.TryParse(package.Version ?? "1.0", out var versionParsed) ? versionParsed : null, 
                Description = package.Description,
                Installers = new List<YamlInstaller>()
            };

            var installer = new YamlInstaller
            {
                InstallerType = YamlInstallerType.msix,
                Scope = YamlScope.user,
                Sha256 = hashSha256,
                SignatureSha256 = hashSha256Signature
            };

            switch (package.ProcessorArchitecture)
            {
                case AppxPackageArchitecture.x86:
                    installer.Arch = YamlArchitecture.x86;
                    break;
                case AppxPackageArchitecture.Arm:
                    installer.Arch = YamlArchitecture.arm;
                    break;
                case AppxPackageArchitecture.x64:
                    installer.Arch = YamlArchitecture.x64;
                    break;
                case AppxPackageArchitecture.Neutral:
                    installer.Arch = YamlArchitecture.Neutral;
                    break;
                case AppxPackageArchitecture.Arm64:
                    installer.Arch = YamlArchitecture.arm64;
                    break;
            }

            yaml.Installers.Add(installer);

            this.YamlUtils.FillGaps(yaml);
            this.SetData(yaml);
            return Task.FromResult(true);
        }

        public Task NewManifest(CancellationToken cancellationToken)
        {
            var newItem = new YamlDefinition
            {
                Version = System.Version.Parse("1.0.0"),
                ManifestVersion = System.Version.Parse("0.1.0"),
            };

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

        private async void OnLoadIdentityFromMsix(object obj)
        {
            if (!this.interactionService.SelectFile("MSIX packages|*.msix|MSIX manifests|AppxManifest.xml", out var selected))
            {
                return;
            }
            
            IAppxFileReader fileReader = null;
            try
            {
                var packageManager = new AppxManifestReader();
                var extension = Path.GetExtension(selected)?.ToLowerInvariant();

                switch (extension)
                {
                    case ".xml":
                        Logger.Info($"Loading manifest file from {selected}...");
                        fileReader = new FileInfoFileReaderAdapter(selected);
                        break;
                    default:
                        Logger.Info($"Loading MSIX package from {selected}...");
                        fileReader = new ZipArchiveFileReaderAdapter(selected);
                        break;
                }

                var manifest = await packageManager.Read(fileReader, CancellationToken.None).ConfigureAwait(false);
                this.Name.CurrentValue = manifest.DisplayName;
                this.Publisher.CurrentValue = manifest.PublisherDisplayName;
                this.Version.CurrentValue = manifest.Version;
                this.Description.CurrentValue = manifest.Description;
                this.Installer.InstallerType.CurrentValue = YamlInstallerType.msix;
                this.Installer.Scope.CurrentValue = YamlScope.user;
                
                var reader = new YamlUtils();
                if (fileReader is ZipArchiveFileReaderAdapter)
                {
                    this.Installer.Sha256.CurrentValue = await reader.CalculateHashAsync(new FileInfo(selected), CancellationToken.None).ConfigureAwait(false);

                    try
                    {
                        this.Installer.SignatureSha256.CurrentValue = await reader.CalculateSignatureHashAsync(new FileInfo(selected), CancellationToken.None).ConfigureAwait(false);
                    }
                    catch (ArgumentException e)
                    {
                        Logger.Warn("SHA256 could not be calculated. This is going to be ignored by the dialog.", e);
                    }
                }
            }
            catch (Exception exception)
            {
                Logger.Warn(exception);
                this.interactionService.ShowError(@$"Could not open MSIX file '{selected}'.", exception);
            }
            finally
            {
                fileReader?.Dispose();
            }
        }

        private void SetData(YamlDefinition definition)
        {
            this.autoId = true;
            this.model = definition;
            this.LicenseUrl.CurrentValue = definition.LicenseUrl;
            this.Name.CurrentValue = definition.Name;
            this.Version.CurrentValue = definition.Version?.ToString();
            this.Publisher.CurrentValue = definition.Publisher;
            this.License.CurrentValue = definition.License;
            this.AppMoniker.CurrentValue = definition.AppMoniker;
            this.Tags.CurrentValue = definition.Tags;
            this.Description.CurrentValue = definition.Description;
            this.Homepage.CurrentValue = definition.Homepage;
            this.MinOSVersion.CurrentValue = definition.MinOperatingSystemVersion?.ToString();
            this.Id.CurrentValue = definition.Id;

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
            this.model.Version = string.IsNullOrEmpty(this.Version.CurrentValue) ? null : System.Version.Parse(this.Version.CurrentValue);
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