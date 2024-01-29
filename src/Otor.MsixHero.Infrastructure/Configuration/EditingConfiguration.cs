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

using System.Runtime.Serialization;

namespace Otor.MsixHero.Infrastructure.Configuration
{
    public class EditingConfiguration : BaseJsonSetting
    {
        public EditingConfiguration()
        {
            this.ManifestEditorType = EditorType.Default;
            this.AppInstallerEditorType = EditorType.Default;
            this.MsixEditorType = EditorType.Default;
            this.PsfEditorType = EditorType.Default;
            this.PowerShellEditorType = EditorType.Ask;

            this.ManifestEditor = new ResolvableFolder.ResolvablePath();
            this.AppInstallerEditor = new ResolvableFolder.ResolvablePath();
            this.MsixEditor = new ResolvableFolder.ResolvablePath();
            this.PsfEditor = new ResolvableFolder.ResolvablePath();
            this.PowerShellEditor = new ResolvableFolder.ResolvablePath();
        }

        [DataMember(Name = "manifestEditorType")]
        public EditorType ManifestEditorType { get; set; }

        [DataMember(Name = "manifestEditorPath")]
        public ResolvableFolder.ResolvablePath ManifestEditor { get; set; }

        [DataMember(Name = "appInstallerEditorType")]
        public EditorType AppInstallerEditorType { get; set; }

        [DataMember(Name = "powerShellEditorType")]
        public EditorType PowerShellEditorType { get; set; }

        [DataMember(Name = "appinstallerEditorPath")]
        public ResolvableFolder.ResolvablePath AppInstallerEditor { get; set; }

        [DataMember(Name = "msixEditorType")]
        public EditorType MsixEditorType { get; set; }

        [DataMember(Name = "msixEditorPath")]
        public ResolvableFolder.ResolvablePath MsixEditor { get; set; }

        [DataMember(Name = "psfEditorType")]
        public EditorType PsfEditorType { get; set; }

        [DataMember(Name = "psfEditorPath")]
        public ResolvableFolder.ResolvablePath PsfEditor { get; set; }

        [DataMember(Name = "powerShellEditorPath")]
        public ResolvableFolder.ResolvablePath PowerShellEditor { get; set; }
    }
}