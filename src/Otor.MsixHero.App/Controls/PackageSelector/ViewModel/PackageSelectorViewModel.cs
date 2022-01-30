// MSIX Hero
// Copyright (C) 2022 Marcin Otorowski
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
using Otor.MsixHero.App.Helpers;
using Otor.MsixHero.App.Mvvm.Changeable;
using Otor.MsixHero.AppInstaller;
using Otor.MsixHero.AppInstaller.Entities;
using Otor.MsixHero.Appx.Editor;
using Otor.MsixHero.Appx.Packaging;
using Otor.MsixHero.Appx.Packaging.Manifest.Enums;
using Dapplo.Log;
using Otor.MsixHero.Infrastructure.Services;

namespace Otor.MsixHero.App.Controls.PackageSelector.ViewModel
{
    public class PackageSelectorViewModel : ChangeableContainer
    {
        private readonly PackageSelectorDisplayMode _displayMode;
        private static readonly LogSource Logger = new();
        private string _customPrompt;
        
        private bool _allowChangingSourcePackage;

        private bool _showPackageTypeSelector;

        public PackageSelectorViewModel(IInteractionService interactionService, PackageSelectorDisplayMode displayMode)
        {
            this._displayMode = displayMode;
            this.Publisher = new ValidatedChangeableProperty<string>(() => Resources.Localization.PackageSelector_PublisherName, AppxValidatorFactory.ValidateSubject());
            this.DisplayPublisher = new ValidatedChangeableProperty<string>(() => Resources.Localization.PackageSelector_PublisherDisplayName, ValidatorFactory.ValidateNotEmptyField());
            this.Name = new ValidatedChangeableProperty<string>(() => Resources.Localization.PackageSelector_Name, AppxValidatorFactory.ValidatePackageName());
            this.DisplayName = new ValidatedChangeableProperty<string>(() => Resources.Localization.PackageSelector_DisplayName, ValidatorFactory.ValidateNotEmptyField());
            this.Version = new ValidatedChangeableProperty<string>(() => Resources.Localization.PackageSelector_Version, AppxValidatorFactory.ValidateVersion());
            this.Architecture = new ChangeableProperty<AppxPackageArchitecture>(AppxPackageArchitecture.Neutral);
            this.PackageType = new ChangeableProperty<PackageType>();
            
            this.InputPath = new ChangeableFileProperty(() => Resources.Localization.PackageSelector_PackagePath, interactionService, ChangeableFileProperty.ValidatePath);

            this.PackageType.ValueChanged += this.PackageTypeOnValueChanged;
            this._showPackageTypeSelector = displayMode.HasFlag(PackageSelectorDisplayMode.ShowTypeSelector);

            this._displayMode = displayMode;
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

            this._allowChangingSourcePackage = displayMode.HasFlag(PackageSelectorDisplayMode.AllowChanging);
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

        public string CustomPrompt { get => this._customPrompt; set => this.SetField(ref this._customPrompt, value); }
        
        public bool ShowPackageTypeSelector
        {
            get => this._showPackageTypeSelector;

            set => this.SetField(ref this._showPackageTypeSelector, value);
        }

        public bool AllowChangingSourcePackage
        {
            get => this._allowChangingSourcePackage;
            set => this.SetField(ref this._allowChangingSourcePackage, value);
        }

        public bool AllowBrowsing { get; private set; }
        
        public bool IsBundle => this.PackageType.CurrentValue == Appx.Packaging.PackageType.Bundle;

        public ChangeableProperty<PackageType> PackageType { get; }
        
        public ChangeableFileProperty InputPath { get; }

        public string LoadButtonCaption
        {
            get
            {
                var packages = this._displayMode.HasFlag(PackageSelectorDisplayMode.AllowPackages);
                var bundles = this._displayMode.HasFlag(PackageSelectorDisplayMode.AllowBundles);
                var manifests = this._displayMode.HasFlag(PackageSelectorDisplayMode.AllowManifests);

                var options = new List<string>();
                if (packages)
                {
                    options.Add(Resources.Localization.PackageSelector_Caption_LoadFromPkg);
                }

                if (bundles)
                {
                    options.Add(Resources.Localization.PackageSelector_Caption_LoadFromBundle);
                }

                if (manifests)
                {
                    options.Add(Resources.Localization.PackageSelector_Caption_LoadFromManifest);
                }

                if (options.Count > 1)
                {
                    return Resources.Localization.PackageSelector_Caption_Start + " " + string.Join(", ", options.Take(options.Count - 1)) + " " + Resources.Localization.PackageSelector_Caption_Separator + " " + options[^1] + " " + Resources.Localization.PackageSelector_Caption_End;
                }

                if (options.Count == 1)
                {
                    return Resources.Localization.PackageSelector_Caption_Start + " " + options[0] + " " + Resources.Localization.PackageSelector_Caption_End;
                }

                return Resources.Localization.PackageSelector_Caption_Start + " " + Resources.Localization.PackageSelector_Caption_End;
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

                if (extension != null)
                {
                    switch (extension.ToLowerInvariant())
                    {
                        case FileConstants.AppxBundleExtension:
                        case FileConstants.MsixBundleExtension:
                            this.PackageType.CurrentValue = Appx.Packaging.PackageType.Bundle;
                            break;
                        
                        case FileConstants.AppxExtension:
                        case FileConstants.MsixExtension:
                            this.PackageType.CurrentValue = Appx.Packaging.PackageType.Package;
                            break;
                    }
                }
                
                try
                {
                    var builder = new AppInstallerBuilder
                    {
                        // ReSharper disable once AssignNullToNotNullAttribute
                        MainPackageSource = new FileInfo((string)e.NewValue)
                    };
                    
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
                    Logger.Warn().WriteLine(string.Format(Resources.Localization.PackageSelector_Error_Manifest_Format, e.NewValue));
                }
            }
        }

        private string GetFilterString(bool allowPackages, bool allowBundles, bool allowManifests)
        {
            var filterBuilder = new DialogFilterBuilder();
            
            if (allowPackages)
            {
                if (!this.ShowPackageTypeSelector || this.PackageType.CurrentValue == Appx.Packaging.PackageType.Package)
                {
                    filterBuilder.AddFilters("*" + FileConstants.MsixExtension, "*" + FileConstants.AppxExtension);
                }
            }

            if (allowBundles)
            {
                if (!this.ShowPackageTypeSelector || this.PackageType.CurrentValue == Appx.Packaging.PackageType.Bundle)
                {
                    // ReSharper disable once StringLiteralTypo
                    filterBuilder.AddFilters("*" + FileConstants.AppxBundleExtension);
                }
            }

            if (allowManifests)
            {
                // ReSharper disable once StringLiteralTypo
                filterBuilder.AddFilters(FileConstants.AppxManifestFile);
            }

            return filterBuilder.BuildFilter();
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
                this._displayMode.HasFlag(PackageSelectorDisplayMode.AllowPackages),
                this._displayMode.HasFlag(PackageSelectorDisplayMode.AllowBundles),
                this._displayMode.HasFlag(PackageSelectorDisplayMode.AllowManifests));
        }
    }
}