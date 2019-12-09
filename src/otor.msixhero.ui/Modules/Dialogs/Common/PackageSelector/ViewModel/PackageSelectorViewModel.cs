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
    public class PackageSelectorViewModel : ChangeableContainer
    {
        private static readonly ILog Logger = LogManager.GetLogger();
        private bool allowChangingSourcePackage = true;
        private bool allowBrowsing = true;
        private bool allowBundles;
        private bool allowManifests = true;
        private bool showPackageTypeSelector;
        private string customPrompt;
        private bool requireFullIdentity = true;

        public PackageSelectorViewModel(IInteractionService interactionService)
        {
            this.Publisher = new ValidatedChangeableProperty<string>(this.ValidateMainPublisher, false);
            this.Name = new ValidatedChangeableProperty<string>(this.ValidateMainName, false);
            this.Version = new ValidatedChangeableProperty<string>(this.ValidateMainVersion, false);
            this.Architecture = new ChangeableProperty<AppInstallerPackageArchitecture>(AppInstallerPackageArchitecture.neutral);

            this.PackageType = new ChangeableProperty<PackageType>();
            this.PackageType.ValueChanged += PackageTypeOnValueChanged;

            this.InputPath = new ChangeableFileProperty(interactionService)
            {
                Validators = new[] { ChangeableFileProperty.ValidatePath },
                Filter = this.GetFilterString()
            };

            this.InputPath.ValueChanged += this.InputPathOnValueChanged;

            this.AddChildren(this.Name, this.Publisher, this.Version, this.Architecture, this.PackageType);
            this.IsValidated = false;
        }

        public string CustomPrompt { get => this.customPrompt; set => this.SetField(ref this.customPrompt, value); }

        public bool AllowBundles
        {
            get => this.allowBundles;
            set
            {
                this.SetField(ref this.allowBundles, value);
                this.InputPath.Filter = this.GetFilterString();

                if (!value)
                {
                    this.ShowPackageTypeSelector = false;
                }
            }
        }

        public bool AllowManifests
        {
            get => this.allowManifests;
            set
            {
                this.SetField(ref this.allowManifests, value);
                this.InputPath.Filter = this.GetFilterString();
            }
        }

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

        public bool AllowBrowsing
        {
            get => this.allowBrowsing;
            set => this.SetField(ref this.allowBrowsing, value);
        }

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

        public ValidatedChangeableProperty<string> Publisher { get; }

        public ValidatedChangeableProperty<string> Version { get; }

        public ChangeableProperty<AppInstallerPackageArchitecture> Architecture { get; }

        public bool RequireFullIdentity
        {
            get => requireFullIdentity;
            set
            {
                this.SetField(ref this.requireFullIdentity, value);

                if (!this.IsValidated)
                {
                    return;
                }

                this.Version.IsValidated = value;
            }
        }

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

        private string ValidateMainPublisher(string newValue)
        {
            if (string.IsNullOrEmpty(newValue))
            {
                return "The publisher may not be empty.";
            }

            return null;
        }

        private string ValidateMainVersion(string newValue)
        {
            if (string.IsNullOrEmpty(newValue))
            {
                return "The version may not be empty.";
            }

            if (!System.Version.TryParse(newValue, out _))
            {
                return $"'{newValue}' is not a valid version.";
            }

            return null;
        }

        private string ValidateMainName(string newValue)
        {
            if (string.IsNullOrEmpty(newValue))
            {
                return "Package name may not be empty.";
            }

            return null;
        }

        private string GetFilterString()
        {
            var extensions = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var names = new StringBuilder();

            extensions.Add("*.msix");
            extensions.Add("*.appx");
            names.Append("Packages|*.msix;*.appx|");

            if (this.AllowBundles)
            {
                extensions.Add("*.appxbundle");
                names.Append("Bundles|*.appxbundle|");
            }

            if (this.AllowManifests)
            {
                extensions.Add("appxmanifest.xml");
                names.Append("Manifest files|appxmanifest.xml|");
            }

            return string.Format("All supported files|{0}|{1}All files|*.*", string.Join(";", extensions.Select(s => s)), names);
        }

        private void PackageTypeOnValueChanged(object sender, ValueChangedEventArgs e)
        {
            this.OnPropertyChanged(nameof(this.IsBundle));
        }
    }
}