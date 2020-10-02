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