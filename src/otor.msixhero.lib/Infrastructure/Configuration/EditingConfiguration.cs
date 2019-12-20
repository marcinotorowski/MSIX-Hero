using System.Runtime.Serialization;

namespace otor.msixhero.lib.Infrastructure.Configuration
{
    public enum EditorType
    {
        Default = 0,
        Custom = 1
    }

    public class EditingConfiguration
    {
        public EditingConfiguration()
        {
            this.ManifestEditorType = EditorType.Default;
            this.AppInstallerEditorType = EditorType.Default;
            this.MsixEditorType = EditorType.Default;
            this.PsfEditorType = EditorType.Default;

            this.ManifestEditor = new ResolvableFolder.ResolvableFolder();
            this.AppInstallerEditor = new ResolvableFolder.ResolvableFolder();
            this.MsixEditor = new ResolvableFolder.ResolvableFolder();
            this.PsfEditor = new ResolvableFolder.ResolvableFolder();
        }

        [DataMember(Name = "manifestEditorType")]
        public EditorType ManifestEditorType { get; set; }

        [DataMember(Name = "manifestEditorPath")]
        public ResolvableFolder.ResolvableFolder ManifestEditor { get; set; }

        [DataMember(Name = "appInstallerEditorType")]
        public EditorType AppInstallerEditorType { get; set; }

        [DataMember(Name = "appinstallerEditorPath")]
        public ResolvableFolder.ResolvableFolder AppInstallerEditor { get; set; }

        [DataMember(Name = "msixEditorType")]
        public EditorType MsixEditorType { get; set; }

        [DataMember(Name = "msixEditorPath")]
        public ResolvableFolder.ResolvableFolder MsixEditor { get; set; }

        [DataMember(Name = "psfEditorType")]
        public EditorType PsfEditorType { get; set; }

        [DataMember(Name = "psfEditorPath")]
        public ResolvableFolder.ResolvableFolder PsfEditor { get; set; }
    }
}