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
using System.Linq;
using Otor.MsixHero.App.Modules.PackageManagement.PackageContent.ViewModel.Items.Psf;
using Otor.MsixHero.App.Mvvm;
using Otor.MsixHero.Appx.Packaging;
using Otor.MsixHero.Appx.Packaging.Installation.Enums;
using Otor.MsixHero.Appx.Packaging.Manifest.Entities;
using Otor.MsixHero.Appx.Psf.Entities.Descriptor;

namespace Otor.MsixHero.App.Modules.PackageManagement.PackageContent.ViewModel.Items
{
    public class AppxApplicationViewModel : NotifyPropertyChanged
    {
        private readonly AppxApplication _model;
        private readonly AppxPackage _package;

        public AppxApplicationViewModel(AppxApplication model, AppxPackage package)
        {
            this._model = model;
            this._package = package;

            this.Proxy = model.Proxy switch
            {
                PsfApplicationProxy psfProxy => new PsfApplicationProxyViewModel(package.RootFolder, psfProxy),
                AdvancedInstallerApplicationProxy advancedInstallerApplicationProxy => new AdvancedInstallerApplicationProxyViewModel(advancedInstallerApplicationProxy),
                MsixHelperApplicationProxy msixHelperApplicationProxy => new MsixHelperApplicationProxyViewModel(msixHelperApplicationProxy),
                _ => null
            };

            this.Services = model.Extensions == null ? null : new ServicesViewModel(model.Extensions);

            this.Type = PackageTypeConverter.GetPackageTypeFrom(this._model.EntryPoint, this._model.Executable, this._model.StartPage, this._package.IsFramework);
            this.Alias = this._model.ExecutionAlias?.Any() == true ? string.Join(", ", this._model.ExecutionAlias.Distinct(StringComparer.OrdinalIgnoreCase)) : null;
        }
        
        public bool Visible => this._model.Visible;

        public string Alias { get; }

        public bool HasPsf => this._model.Proxy is PsfApplicationProxy;

        public bool HasProxy => this._model.Proxy != null;
        
        public BaseApplicationProxyViewModel Proxy { get; }

        public ServicesViewModel Services { get; }

        public string DisplayName => this._model.DisplayName;

        public byte[] Image => this._model.Logo;
        
        public string Id => this._model.Id;

        public string TileColor => this._model.BackgroundColor;

        public string Start
        {
            get
            {
                switch (PackageTypeConverter.GetPackageTypeFrom(this._model.EntryPoint, this._model.Executable, this._model.StartPage, this._package.IsFramework))
                {
                    case MsixPackageType.Win32:
                    case MsixPackageType.Win32Psf:
                    case MsixPackageType.Win32AiStub:
                    case MsixPackageType.MsixHelper:
                        return this._model.Executable;
                    case MsixPackageType.Web:
                        return this._model.StartPage;
                    default:
                        if (string.IsNullOrEmpty(this._model.EntryPoint))
                        {
                            return this._model.Executable;
                        }

                        return this._model.EntryPoint;
                }
            }
        }

        public string EntryPoint => this.Type == MsixPackageType.Uwp ? this._model.EntryPoint : null;

        public bool HasEntryPoint => this.Type == MsixPackageType.Uwp;

        public MsixPackageType Type { get; }
    }
}
