using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using otor.msixhero.lib.BusinessLayer.Appx.Manifest;
using otor.msixhero.lib.BusinessLayer.Appx.Manifest.FileReaders;
using otor.msixhero.lib.BusinessLayer.Winget;
using otor.msixhero.lib.Domain.Appx.Manifest.Full;
using otor.msixhero.lib.Domain.Winget;
using otor.msixhero.lib.Infrastructure;
using otor.msixhero.lib.Infrastructure.Progress;
using otor.msixhero.ui.Domain;

namespace otor.msixhero.ui.Modules.Dialogs.Winget.ViewModel
{
    public class WingetDefinitionViewModel : ChangeableContainer
    {
        private readonly IInteractionService interactionService;
        protected readonly YamlWriter YamlWriter = new YamlWriter();
        protected readonly YamlReader YamlReader = new YamlReader();
        protected readonly YamlUtils YamlUtils = new YamlUtils();
        private bool isLoading;

        public WingetDefinitionViewModel(IInteractionService interactionService)
        {
            this.interactionService = interactionService;
            this.AddChildren(
                this.Name = new ValidatedChangeableProperty<string>(true, ValidatorFactory.ValidateNotEmptyField("Package name")),
                this.Version = new ValidatedChangeableProperty<string>(true, ValidatorFactory.ValidateVersion(true, "Version")),
                this.InstallerType = new ValidatedChangeableProperty<YamlInstallerType>(true),
                this.Installer = new WingetInstallerViewModel(interactionService),
                this.Author = new ChangeableProperty<string>(),
                this.LicenseUrl = new ValidatedChangeableProperty<string>(true, ValidatorFactory.ValidateUrl(false, "License URL")),
                this.Tags = new ChangeableProperty<string>(),
                this.AppMoniker = new ChangeableProperty<string>(),
                this.Homepage = new ValidatedChangeableProperty<string>(true, ValidatorFactory.ValidateUrl(false, "Homepage URL")),
                this.Commands = new ChangeableProperty<string>(),
                this.Id = new ValidatedChangeableProperty<string>(true, ValidateId),
                this.Description = new ChangeableProperty<string>(),
                this.Publisher = new ValidatedChangeableProperty<string>(true, ValidatorFactory.ValidateNotEmptyField("Publisher name")),
                this.License = new ChangeableProperty<string>(),
                this.MinOSVersion = new ValidatedChangeableProperty<string>(true, ValidatorFactory.ValidateVersion(false, "Minimum OS version")));

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

        public ChangeableProperty<string> Author { get; }

        public ValidatedChangeableProperty<string> Id { get; }

        public ValidatedChangeableProperty<string> Version { get; }

        public ChangeableProperty<string> Commands { get; }

        // ReSharper disable once InconsistentNaming
        public ValidatedChangeableProperty<string> MinOSVersion { get; }

        public ChangeableProperty<string> License { get; }

        public ChangeableProperty<string> Tags { get; }

        public ValidatedChangeableProperty<string> Homepage { get; }

        public ChangeableProperty<string> Description { get; }

        public ValidatedChangeableProperty<string> LicenseUrl { get; }

        public ValidatedChangeableProperty<YamlInstallerType> InstallerType { get; }

        public IList<YamlInstaller> OtherInstallers { get; private set; }

        public WingetInstallerViewModel Installer { get; }

        public bool IsLoading
        {
            get => this.isLoading;
            set => this.SetField(ref this.isLoading, value);
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
                Author = package.PublisherDisplayName,
                MinOSVersion = "10.0.0",
                Version = package.Version,
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
                InstallerType = YamlInstallerType.msix,
                Version = "1.0.0"
            };

            this.SetData(newItem);

            return Task.FromResult(true);
        }

        private void SetData(YamlDefinition definition)
        {
            this.LicenseUrl.CurrentValue = definition.LicenseUrl;
            this.Name.CurrentValue = definition.Name;
            this.Version.CurrentValue = definition.Version;
            this.Publisher.CurrentValue = definition.Publisher;
            this.License.CurrentValue = definition.License;
            this.AppMoniker.CurrentValue = definition.AppMoniker;
            this.Tags.CurrentValue = definition.Tags;
            this.Description.CurrentValue = definition.Description;
            this.Homepage.CurrentValue = definition.Homepage;
            this.MinOSVersion.CurrentValue = definition.MinOSVersion;
            this.Author.CurrentValue = definition.Author;
            this.Id.CurrentValue = definition.Id;
            this.Commands.CurrentValue = definition.Commands;
            this.InstallerType.CurrentValue = definition.InstallerType ?? YamlInstallerType.none;

            if (definition.Installers?.Any() == true)
            {
                this.OtherInstallers = definition.Installers.Skip(1).ToList();
                this.Installer.SetData(definition.Installers.First());
            }
            else
            {
                this.OtherInstallers = null;
                var newItem = new YamlInstaller
                {
                    InstallerType = YamlInstallerType.msix,
                    Arch = YamlArchitecture.x64,
                    Scope = YamlScope.machine
                };

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

            var yamlDefinition = new YamlDefinition
            {
                Name = this.Name.CurrentValue,
                AppMoniker = this.AppMoniker.CurrentValue,
                Author = this.Author.CurrentValue,
                Description = this.Description.CurrentValue,
                License = this.License.CurrentValue,
                Homepage = this.Homepage.CurrentValue,
                Id = this.Id.CurrentValue,
                Publisher = this.Publisher.CurrentValue,
                MinOSVersion = this.MinOSVersion.CurrentValue,
                Tags = this.Tags.CurrentValue,
                Version = this.Version.CurrentValue,
                LicenseUrl = this.LicenseUrl.CurrentValue,
                Commands = this.Commands.CurrentValue
            };

            if (this.InstallerType.CurrentValue != YamlInstallerType.none)
            {
                yamlDefinition.InstallerType = this.InstallerType.CurrentValue;
            }

            var installers = new List<YamlInstaller>();

            this.Installer.Commit();
            installers.Add(this.Installer.Model);

            if (this.OtherInstallers.Any())
            {
                installers.AddRange(this.OtherInstallers);
            }

            if (installers.Any())
            {
                yamlDefinition.Installers = installers;
            }

            var fileInfo = new FileInfo(fileName);
            if (fileInfo.Directory != null && !fileInfo.Directory.Exists)
            {
                fileInfo.Directory.Create();
            }

            using (var fs = File.OpenWrite(fileName))
            {
                await this.YamlWriter.WriteAsync(yamlDefinition, fs, cancellationToken).ConfigureAwait(false);
            }

            return true;
        }
    }
}