using Otor.MsixHero.Appx.Updates.Serialization.ComparePackage;
using Otor.MsixHero.Ui.ViewModel;

namespace Otor.MsixHero.Ui.Modules.Dialogs.UpdateImpact.ViewModel.Items
{
    public class FileViewModel : NotifyPropertyChanged
    {
        private FileViewModel(FileType type)
        {
            this.Type = type;
        }

        public FileViewModel(ChangedFile changedFile) : this(FileType.Changed)
        {
            this.Name = changedFile.Name;
            this.UpdateImpact = changedFile.UpdateImpact;
            this.SizeDifference = changedFile.SizeDifference;
        }

        public FileViewModel(FileType type, File file) : this(type)
        {
            this.Name = file.Name;
            this.UpdateImpact = file.UpdateImpact;
            this.SizeDifference = type == FileType.Unchanged ? 0 : file.Size;
        }

        public FileType Type { get; }

        public string Name { get; }

        public long? SizeDifference { get; }

        public long? UpdateImpact { get; }
    }
}
