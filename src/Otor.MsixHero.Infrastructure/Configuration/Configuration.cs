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

using System.Runtime.Serialization;

namespace Otor.MsixHero.Infrastructure.Configuration
{
    [DataContract(Name = "configuration")]
    public class Configuration : BaseJsonSetting
    {
        public Configuration()
        {
            this.Packages = new PackagesConfiguration();
            this.Events = new EventsConfiguration();
            this.Containers = new ContainersConfiguration();
            this.Signing = new SigningConfiguration();
            this.Packer = new PackerConfiguration();
            this.AppInstaller = new AppInstallerConfiguration();
            this.AppAttach = new AppAttachConfiguration();
            this.Editing = new EditingConfiguration();
            this.UiConfiguration = new UiConfiguration();
            this.Advanced = new AdvancedConfiguration();
        }
        
        // ReSharper disable once StringLiteralTypo
        [DataMember(Name = "appattach")]
        public AppAttachConfiguration AppAttach { get; set; }

        [DataMember(Name = "editing")]
        public EditingConfiguration Editing { get; set; }

        [DataMember(Name = "ui")]
        public UiConfiguration UiConfiguration { get; set; }

        [DataMember(Name = "packages")]
        public PackagesConfiguration Packages { get; set; }

        [DataMember(Name = "events")]
        public EventsConfiguration Events { get; set; }

        [DataMember(Name = "containers")]
        public ContainersConfiguration Containers { get; set; }

        [DataMember(Name = "signing")]
        public SigningConfiguration Signing { get; set; }

        [DataMember(Name = "packer")]
        public PackerConfiguration Packer { get; set; }

        [DataMember(Name ="appinstaller")]
        public AppInstallerConfiguration AppInstaller { get; set; }

        [DataMember(Name = "advanced")]
        public AdvancedConfiguration Advanced { get; set; }

        [DataMember(Name = "update")]
        public UpdateConfiguration Update { get; set; }

        [DataMember(Name = "verboseLog")]
        public bool VerboseLogging { get; set; }

        [DataMember(Name = "revision")]
        public int Revision { get; set; }
    }
}
