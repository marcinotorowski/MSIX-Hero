using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using otor.msixhero.lib.BusinessLayer.Appx.AppInstaller;
using otor.msixhero.lib.Domain.Appx.AppInstaller;
using otor.msixhero.lib.Infrastructure;
using otor.msixhero.lib.Infrastructure.Logging;
using otor.msixhero.ui.Domain;

namespace otor.msixhero.ui.Modules.Dialogs.Common.PackageSelector.ViewModel
{
    [Flags]
    public enum PackageSelectorDisplayMode
    {
        ShowDisplayName = 1,
        ShowActualName = 2,
        AllowPackages = 4,
        AllowBundles = 8,
        AllowManifests = 16,
        AllowAllPackageTypes = AllowPackages | AllowBundles | AllowManifests,
        ShowTypeSelector = 32,
        AllowChanging = 64,
        AllowBrowsing = 128,
        RequireFullIdentity = 256
    }

    public class PackageSelectorViewModel : ChangeableContainer
    {
        private readonly PackageSelectorDisplayMode displayMode;
        private static readonly ILog Logger = LogManager.GetLogger();
        private string customPrompt;
        private bool requireFullIdentity = true;

        private bool allowChangingSourcePackage;

        private bool showPackageTypeSelector;

        public PackageSelectorViewModel(IInteractionService interactionService, PackageSelectorDisplayMode displayMode)
        {
            this.displayMode = displayMode;
            this.Publisher = new ValidatedChangeableProperty<string>(this.ValidateSubject, false);
            this.DisplayPublisher = new ValidatedChangeableProperty<string>(ValidatedChangeableProperty<string>.ValidateNotNull, false);
            this.Name = new ValidatedChangeableProperty<string>(this.ValidateName, false);
            this.DisplayName = new ValidatedChangeableProperty<string>(ValidatedChangeableProperty<string>.ValidateNotNull, false);
            this.Version = new ValidatedChangeableProperty<string>(this.ValidateVersion, false);
            this.Architecture = new ChangeableProperty<AppInstallerPackageArchitecture>(AppInstallerPackageArchitecture.neutral);

            this.PackageType = new ChangeableProperty<PackageType>();
            this.PackageType.ValueChanged += this.PackageTypeOnValueChanged;

            this.showPackageTypeSelector = displayMode.HasFlag(PackageSelectorDisplayMode.ShowTypeSelector);

            this.InputPath = new ChangeableFileProperty(interactionService)
            {
                Validators = new[] { ChangeableFileProperty.ValidatePath }
            };

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

            this.IsValidated = false;
            if (displayMode.HasFlag(PackageSelectorDisplayMode.RequireFullIdentity))
            {
                this.RequireFullIdentity = true;
                this.AddChildren(this.Version, this.Architecture);
            }

            this.AddChildren(this.PackageType);
        }

        public bool ShowDisplayNames { get; private set; }

        public bool ShowActualNames { get; private set; }

        public string CustomPrompt { get => this.customPrompt; set => this.SetField(ref this.customPrompt, value); }

        public bool AllowBundles { get; private set; }

        public bool AllowManifests { get; private set; }

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
        
        public bool IsBundle => this.PackageType.CurrentValue == lib.BusinessLayer.Appx.AppInstaller.PackageType.Bundle;

        public ChangeableProperty<PackageType> PackageType { get; }
        
        public ChangeableFileProperty InputPath { get; }

        public string LoadButtonCaption
        {
            get
            {
                if (this.AllowManifests)
                {
                    return "Load from a package or manifest...";
                }
                else
                {
                    return "Load from a package...";
                }
            }
        }

        public ValidatedChangeableProperty<string> Name { get; }

        public ValidatedChangeableProperty<string> DisplayName { get; }

        public ValidatedChangeableProperty<string> Publisher { get; }

        public ValidatedChangeableProperty<string> DisplayPublisher { get; }

        public ValidatedChangeableProperty<string> Version { get; }

        public ChangeableProperty<AppInstallerPackageArchitecture> Architecture { get; }

        public bool RequireFullIdentity { get; private set; }

        private void InputPathOnValueChanged(object sender, ValueChangedEventArgs e)
        {
            if (string.IsNullOrEmpty((string)e.NewValue))
            {
                this.Name.CurrentValue = null;
                this.Version.CurrentValue = null;
                this.Publisher.CurrentValue = null;
                this.Architecture.CurrentValue = AppInstallerPackageArchitecture.neutral;
            }
            else
            {
                var extension = Path.GetExtension((string)e.NewValue);
                if (string.Equals(extension, ".appxbundle", StringComparison.OrdinalIgnoreCase))
                {
                    this.PackageType.CurrentValue = lib.BusinessLayer.Appx.AppInstaller.PackageType.Bundle;
                }
                else if (string.Equals(extension, ".appx", StringComparison.OrdinalIgnoreCase) || string.Equals(extension, ".msix", StringComparison.OrdinalIgnoreCase))
                {
                    this.PackageType.CurrentValue = lib.BusinessLayer.Appx.AppInstaller.PackageType.Package;
                }

                try
                {
                    var builder = new AppInstallerBuilder();
                    builder.MainPackageSource = new FileInfo((string)e.NewValue);
                    var config = builder.Build();

                    this.Name.CurrentValue = config.MainPackage.Name;
                    this.Version.CurrentValue = config.MainPackage.Version;
                    this.Publisher.CurrentValue = config.MainPackage.Publisher;
                    this.Architecture.CurrentValue = config.MainPackage.Architecture;
                }
                catch (Exception)
                {
                    Logger.Warn($"Could not read value from MSIX manifest {e.NewValue}");
                }
            }
        }

        private string ValidateSubject(string newValue)
        {
            if (string.IsNullOrEmpty(newValue))
            {
                return "The publisher may not be empty.";
            }

            if (!newValue.StartsWith("CN=", StringComparison.OrdinalIgnoreCase))
            {
                return "Publisher name must start with CN=";
            }

            return null;
        }

        private string ValidateVersion(string newValue)
        {
            if (string.IsNullOrEmpty(newValue))
            {
                return "The version may not be empty.";
            }

            if (!System.Version.TryParse(newValue, out var version))
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
                if (!this.ShowPackageTypeSelector || this.PackageType.CurrentValue ==  lib.BusinessLayer.Appx.AppInstaller.PackageType.Package)
                {
                    extensions.Add("*.msix");
                    extensions.Add("*.appx");
                    names.Append("Packages|*.msix;*.appx|");
                    supportedExtensionsCount++;
                }
            }

            if (allowBundles)
            {
                if (!this.ShowPackageTypeSelector || this.PackageType.CurrentValue ==  lib.BusinessLayer.Appx.AppInstaller.PackageType.Bundle)
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