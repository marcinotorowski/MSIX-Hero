using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Otor.MsixHero.AppInstaller;
using Otor.MsixHero.AppInstaller.Entities;
using Otor.MsixHero.Appx.Packaging;
using Otor.MsixHero.Appx.Packaging.Manifest.Enums;
using Otor.MsixHero.Infrastructure.Logging;
using Otor.MsixHero.Infrastructure.Services;
using Otor.MsixHero.Ui.Domain;

namespace Otor.MsixHero.Ui.Modules.Dialogs.Common.PackageSelector.ViewModel
{
    public class PackageSelectorViewModel : ChangeableContainer
    {
        private readonly PackageSelectorDisplayMode displayMode;
        private static readonly ILog Logger = LogManager.GetLogger();
        private string customPrompt;
        
        private bool allowChangingSourcePackage;

        private bool showPackageTypeSelector;

        public PackageSelectorViewModel(IInteractionService interactionService, PackageSelectorDisplayMode displayMode)
        {
            this.displayMode = displayMode;
            this.Publisher = new ValidatedChangeableProperty<string>(ValidatorFactory.ValidateSubject());
            this.DisplayPublisher = new ValidatedChangeableProperty<string>(ValidatorFactory.ValidateNotEmptyField());
            this.Name = new ValidatedChangeableProperty<string>(this.ValidateName);
            this.DisplayName = new ValidatedChangeableProperty<string>(ValidatorFactory.ValidateNotEmptyField());
            this.Version = new ValidatedChangeableProperty<string>(this.ValidateVersion);
            this.Architecture = new ChangeableProperty<AppxPackageArchitecture>(AppxPackageArchitecture.Neutral);
            this.PackageType = new ChangeableProperty<PackageType>();

            this.Publisher.DisplayName = "Publisher name";
            this.DisplayPublisher.DisplayName = "Publisher display name";
            this.Name.DisplayName = "Package name";
            this.DisplayName.DisplayName = "Package display name";
            this.Version.DisplayName = "Package version";

            this.InputPath = new ChangeableFileProperty(interactionService)
            {
                Validators = new[] { ChangeableFileProperty.ValidatePath }
            };

            this.PackageType.ValueChanged += this.PackageTypeOnValueChanged;
            this.showPackageTypeSelector = displayMode.HasFlag(PackageSelectorDisplayMode.ShowTypeSelector);

            this.displayMode = displayMode;
            this.SetInputFilter();

            this.InputPath.ValueChanged += this.InputPathOnValueChanged;

            if (displayMode.HasFlag(PackageSelectorDisplayMode.ShowActualName))
            {
                this.ShowActualNames = true;
                this.AddChildren(this.Name, this.Publisher);
            }

            if (displayMode.HasFlag(PackageSelectorDisplayMode.ShowDisplayName))
            {
                this.ShowDisplayNames = true;
                this.AddChildren(this.DisplayName, this.DisplayPublisher);
            }

            this.allowChangingSourcePackage = displayMode.HasFlag(PackageSelectorDisplayMode.AllowChanging);
            this.AllowBrowsing = displayMode.HasFlag(PackageSelectorDisplayMode.AllowBrowsing);

            if (displayMode.HasFlag(PackageSelectorDisplayMode.RequireFullIdentity))
            {
                this.RequireFullIdentity = true;
                this.RequireArchitecture = true;
                this.RequireVersion = true;
                this.AddChildren(this.Version, this.Architecture);
            }
            else if (displayMode.HasFlag(PackageSelectorDisplayMode.RequireArchitecture))
            {
                this.RequireFullIdentity = true;
                this.RequireArchitecture = true;
                this.AddChildren(this.Architecture);
            }
            else if (displayMode.HasFlag(PackageSelectorDisplayMode.RequireVersion))
            {
                this.RequireFullIdentity = true;
                this.RequireVersion = true;
                this.AddChildren(this.Version);
            }

            this.AddChildren(this.PackageType);
        }

        public bool ShowDisplayNames { get; private set; }

        public bool ShowActualNames { get; private set; }

        public string CustomPrompt { get => this.customPrompt; set => this.SetField(ref this.customPrompt, value); }
        
        public bool ShowPackageTypeSelector
        {
            get => this.showPackageTypeSelector;

            set => this.SetField(ref this.showPackageTypeSelector, value);
        }

        public bool AllowChangingSourcePackage
        {
            get => this.allowChangingSourcePackage;
            set => this.SetField(ref this.allowChangingSourcePackage, value);
        }

        public bool AllowBrowsing { get; private set; }
        
        public bool IsBundle => this.PackageType.CurrentValue == Appx.Packaging.PackageType.Bundle;

        public ChangeableProperty<PackageType> PackageType { get; }
        
        public ChangeableFileProperty InputPath { get; }

        public string LoadButtonCaption
        {
            get
            {
                var packages = this.displayMode.HasFlag(PackageSelectorDisplayMode.AllowPackages);
                var bundles = this.displayMode.HasFlag(PackageSelectorDisplayMode.AllowBundles);
                var manifests = this.displayMode.HasFlag(PackageSelectorDisplayMode.AllowManifests);

                var options = new List<string>();
                if (packages)
                {
                    options.Add("a package");
                }

                if (bundles)
                {
                    options.Add("a bundle");
                }

                if (manifests)
                {
                    options.Add("a manifest");
                }

                var part1 = string.Join(", ", options.Take(options.Count - 1));
                return "Load from " + string.Join(" or ", new[] {part1}.Concat(options.Skip(options.Count - 1)));
            }
        }

        public ValidatedChangeableProperty<string> Name { get; }

        public ValidatedChangeableProperty<string> DisplayName { get; }

        public ValidatedChangeableProperty<string> Publisher { get; }

        public ValidatedChangeableProperty<string> DisplayPublisher { get; }

        public ValidatedChangeableProperty<string> Version { get; }

        public ChangeableProperty<AppxPackageArchitecture> Architecture { get; }

        public bool RequireFullIdentity { get; private set; }
        
        public bool RequireVersion { get; private set; }
        
        public bool RequireArchitecture { get; private set; }

        public EventHandler<PackageFileChangedEventArgs> PackageFileChanged;

        public class PackageFileChangedEventArgs : EventArgs
        {
            public PackageFileChangedEventArgs(string filePath, string name, string publisher, string version, PackageType packageType, AppxPackageArchitecture architecture)
            {
                this.FilePath = filePath;
                this.Name = name;
                this.Publisher = publisher;
                this.Version = version;
                this.PackageType = packageType;
                this.Architecture = architecture;
            }

            public string FilePath { get; private set; }

            public string Name { get; private set; }

            public string Publisher { get; private set; }

            public string Version { get; private set; }

            public PackageType PackageType { get; private set; }

            public AppxPackageArchitecture Architecture { get; private set; }
        }


        private void InputPathOnValueChanged(object sender, ValueChangedEventArgs e)
        {
            if (string.IsNullOrEmpty((string)e.NewValue))
            {
                this.Name.CurrentValue = null;
                this.Version.CurrentValue = null;
                this.Publisher.CurrentValue = null;
                this.Architecture.CurrentValue = AppxPackageArchitecture.Neutral;
                this.PackageFileChanged?.Invoke(this, new PackageFileChangedEventArgs(null, null, null, null, 0, 0));
            }
            else
            {
                var extension = Path.GetExtension((string)e.NewValue);
                this.PackageType.CurrentValue = 0;
                if (string.Equals(extension, ".appxbundle", StringComparison.OrdinalIgnoreCase))
                {
                    this.PackageType.CurrentValue = Appx.Packaging.PackageType.Bundle;
                }
                else if (string.Equals(extension, ".appx", StringComparison.OrdinalIgnoreCase) || string.Equals(extension, ".msix", StringComparison.OrdinalIgnoreCase))
                {
                    this.PackageType.CurrentValue = Appx.Packaging.PackageType.Package;
                }

                try
                {
                    var builder = new AppInstallerBuilder();
                    // ReSharper disable once AssignNullToNotNullAttribute
                    builder.MainPackageSource = new FileInfo((string)e.NewValue);
                    var config = builder.Build();

                    this.Name.CurrentValue = config.MainPackage.Name;
                    this.Version.CurrentValue = config.MainPackage.Version;
                    this.Publisher.CurrentValue = config.MainPackage.Publisher;

                    switch (config.MainPackage.Architecture)
                    {
                        case AppInstallerPackageArchitecture.x86:
                            this.Architecture.CurrentValue = AppxPackageArchitecture.x86;
                            break;
                        case AppInstallerPackageArchitecture.x64:
                            this.Architecture.CurrentValue = AppxPackageArchitecture.x64;
                            break;
                        case AppInstallerPackageArchitecture.arm:
                            this.Architecture.CurrentValue = AppxPackageArchitecture.Arm;
                            break;
                        case AppInstallerPackageArchitecture.arm64:
                            this.Architecture.CurrentValue = AppxPackageArchitecture.Arm64;
                            break;
                        case AppInstallerPackageArchitecture.neutral:
                            this.Architecture.CurrentValue = AppxPackageArchitecture.Neutral;
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }

                    this.PackageFileChanged?.Invoke(
                        this, 
                        new PackageFileChangedEventArgs(
                            (string)e.NewValue,
                            config.MainPackage.Name,
                            config.MainPackage.Publisher,
                            config.MainPackage.Version,
                            this.PackageType.CurrentValue,
                            this.Architecture.CurrentValue
                        ));
                }
                catch (Exception)
                {
                    Logger.Warn($"Could not read value from MSIX manifest {e.NewValue}");
                }
            }
        }

        private string ValidateVersion(string newValue)
        {
            if (string.IsNullOrEmpty(newValue))
            {
                return "The version may not be empty.";
            }

            if (!System.Version.TryParse(newValue, out _))
            {
                return $"'{newValue}' is not a valid version.";
            }

            if (newValue.Split('.').Length != 4)
            {
                return "The version must have 4 units (for example 1.2.3.4).";
            }

            return null;
        }

        private string ValidateName(string newValue)
        {
            if (string.IsNullOrEmpty(newValue))
            {
                return "Package name may not be empty.";
            }

            return null;
        }

        private string GetFilterString(bool allowPackages, bool allowBundles, bool allowManifests)
        {
            var extensions = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var names = new StringBuilder();
            var supportedExtensionsCount = 0;

            if (allowPackages)
            {
                if (!this.ShowPackageTypeSelector || this.PackageType.CurrentValue == Appx.Packaging.PackageType.Package)
                {
                    extensions.Add("*.msix");
                    extensions.Add("*.appx");
                    names.Append("Packages|*.msix;*.appx|");
                    supportedExtensionsCount++;
                }
            }

            if (allowBundles)
            {
                if (!this.ShowPackageTypeSelector || this.PackageType.CurrentValue == Appx.Packaging.PackageType.Bundle)
                {
                    extensions.Add("*.appxbundle");
                    names.Append("Bundles|*.appxbundle|");
                    supportedExtensionsCount++;
                }
            }

            if (allowManifests)
            {
                extensions.Add("appxmanifest.xml");
                names.Append("Manifest files|appxmanifest.xml|");
                supportedExtensionsCount++;
            }

            if (supportedExtensionsCount > 1)
            {
                return $"All supported files|{string.Join(";", extensions)}|{names}All files|*.*";
            }

            return $"{names}All files|*.*";
        }

        private void PackageTypeOnValueChanged(object sender, ValueChangedEventArgs e)
        {
            this.OnPropertyChanged(nameof(this.IsBundle));
            this.OnPropertyChanged(nameof(this.LoadButtonCaption));
            this.SetInputFilter();
        }

        private void SetInputFilter()
        {
            this.InputPath.Filter = this.GetFilterString(
                this.displayMode.HasFlag(PackageSelectorDisplayMode.AllowPackages),
                this.displayMode.HasFlag(PackageSelectorDisplayMode.AllowBundles),
                this.displayMode.HasFlag(PackageSelectorDisplayMode.AllowManifests));
        }
    }
}