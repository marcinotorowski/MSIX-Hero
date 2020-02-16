using System.Runtime.Serialization;

namespace otor.msixhero.lib.Infrastructure.Configuration
{
    public enum EditorType
    {
        Default = 0,
        Custom = 1,
        Ask = 2
    }

    public class EditingConfiguration
    {
        public EditingConfiguration()
        {
            this.ManifestEditorType = EditorType.Default;
            this.AppInstallerEditorType = EditorType.Default;
            this.MsixEditorType = EditorType.Default;
            this.PsfEditorType = EditorType.Default;

            this.ManifestEditor = new ResolvableFolder.ResolvablePath();
            this.AppInstallerEditor = new ResolvableFolder.ResolvablePath();
            this.MsixEditor = new ResolvableFolder.ResolvablePath();
            this.PsfEditor = new ResolvableFolder.ResolvablePath();
        }

        [DataMember(Name = "manifestEditorType")]
        public EditorType ManifestEditorType { get; set; }

        [DataMember(Name = "manifestEditorPath")]
        public ResolvableFolder.ResolvablePath ManifestEditor { get; set; }

        [DataMember(Name = "appInstallerEditorType")]
        public EditorType AppInstallerEditorType { get; set; }

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
    }
}