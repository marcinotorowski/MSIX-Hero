using System;
using System.Collections.Generic;
using System.Text;
using otor.msixhero.lib.Domain.Appx.UpdateImpact.ComparePackage;
using otor.msixhero.ui.ViewModel;

namespace otor.msixhero.ui.Modules.Dialogs.UpdateImpact.ViewModel.Items
{
    public enum FileType
    {
        Added,
        Changed,
        Unchanged,
        Deleted
    }

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
