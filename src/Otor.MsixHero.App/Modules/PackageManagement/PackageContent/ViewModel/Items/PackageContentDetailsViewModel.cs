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

using System.Collections.ObjectModel;
using System.Linq;
using Otor.MsixHero.App.Modules.PackageManagement.PackageContent.ViewModel.Items.Psf;
using Otor.MsixHero.App.Mvvm;
using Otor.MsixHero.Appx.Packaging.Manifest.Entities;
using Otor.MsixHero.Appx.Packaging.Manifest.Entities.Build;
using Otor.MsixHero.Appx.Psf.Entities.Descriptor;

namespace Otor.MsixHero.App.Modules.PackageManagement.PackageContent.ViewModel.Items
{
    public class PackageContentDetailsViewModel : NotifyPropertyChanged
    {
        public PackageContentDetailsViewModel(AppxPackage model, string filePath = null)
        {
            this.Model = model;
            this.DisplayName = model.DisplayName;
            this.Description = model.Description;
            this.Publisher = model.Publisher;
            this.FamilyName = model.FamilyName;
            this.Architecture = model.ProcessorArchitecture.ToString();
            this.PackageFullName = model.FullName;
            this.PublisherDisplayName = model.PublisherDisplayName;
            this.Version = model.Version;
            this.Logo = model.Logo;
            
            this.OperatingSystemDependencies = new ObservableCollection<OperatingSystemDependencyViewModel>();
            this.Applications = new ObservableCollection<AppxApplicationViewModel>();
            this.PackageDependencies = new ObservableCollection<PackageDependencyViewModel>();
            
            if (model.OperatingSystemDependencies != null)
            {
                foreach (var item in model.OperatingSystemDependencies)
                {
                    this.OperatingSystemDependencies.Add(new OperatingSystemDependencyViewModel(item));
                }
            }

            if (model.PackageDependencies != null)
            {
                foreach (var item in model.PackageDependencies)
                {
                    this.PackageDependencies.Add(new PackageDependencyViewModel(item));
                }
            }

            this.HasOperatingSystemDependencies = this.OperatingSystemDependencies.Any();
            this.HasPackageDependencies = this.PackageDependencies.Any();

            this.ScriptsCount = 0;

            if (model.Applications != null)
            {
                foreach (var item in model.Applications)
                {
                    if (string.IsNullOrEmpty(this.TileColor))
                    {
                        this.TileColor = item.BackgroundColor;
                    }

                    this.Applications.Add(new AppxApplicationViewModel(item, model));

                    if (item.Proxy is PsfApplicationProxy psfProxy)
                    {
                        this.ScriptsCount += psfProxy.Scripts?.Count ?? 0;
                    }
                }
            }
            
            this.Fixups = new ObservableCollection<AppxApplicationViewModel>(this.Applications.Where(a => a.Proxy is PsfApplicationProxyViewModel psf && (psf.HasFileRedirections || psf.HasTracing || psf.HasOtherFixups)));
            
            // 1) fixup count is the sum of all individual file redirections…
            this.FixupsCount = this.Fixups.Select(f => f.Proxy).OfType<PsfApplicationProxyViewModel>().SelectMany(s => s.FileRedirections).Select(s => s.Exclusions.Count + s.Inclusions.Count).Sum();
            
            // 2) plus additionally number of apps that have tracing
            this.FixupsCount += this.Applications.Count(a => a.Proxy is PsfApplicationProxyViewModel psf && psf.HasTracing);
            
            this.BuildInfo = model.BuildInfo;

            if (string.IsNullOrEmpty(this.TileColor))
            {
                this.TileColor = "#666666";
            }

            this.Capabilities = new CapabilitiesViewModel(model.Capabilities);
            this.PackageIntegrity = model.PackageIntegrity;
            this.RootDirectory = filePath;
        }

        public string RootDirectory { get; }

        public AppxPackage Model { get; private set; }
        
        public bool PackageIntegrity { get; }

        public CapabilitiesViewModel Capabilities { get; }

        public string PackageFullName { get; }

        public string Description { get; }

        public string PublisherDisplayName { get; }

        public string Publisher { get; }

        public string TileColor { get; }

        public string DisplayName { get; }

        public byte[] Logo { get; }

        public string FamilyName { get; }

        public string Architecture { get; }

        public string Version { get; }
        
        public ObservableCollection<OperatingSystemDependencyViewModel> OperatingSystemDependencies { get; }

        public bool HasOperatingSystemDependencies { get; }

        public bool HasPackageDependencies { get; }

        public ObservableCollection<PackageDependencyViewModel> PackageDependencies { get; }
        
        public ObservableCollection<AppxApplicationViewModel> Applications { get; }

        public ObservableCollection<AppxApplicationViewModel> Fixups { get; }
        
        public BuildInfo BuildInfo { get; }

        public int FixupsCount { get; }
        
        public int ScriptsCount { get; }

        public bool HasBuildInfo => this.BuildInfo != null;
    }
}
