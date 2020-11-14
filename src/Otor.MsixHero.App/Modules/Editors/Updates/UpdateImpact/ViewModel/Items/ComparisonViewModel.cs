using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Data;
using Otor.MsixHero.App.Controls.PackageExpert.ViewModels.Items;
using Otor.MsixHero.App.Mvvm;
using Otor.MsixHero.Appx.Updates.Entities;
using Otor.MsixHero.Appx.Updates.Entities.Blocks;
using Otor.MsixHero.Appx.Updates.Serialization.ComparePackage;

namespace Otor.MsixHero.App.Modules.Editors.Updates.UpdateImpact.ViewModel.Items
{
    public class ComparisonViewModel : NotifyPropertyChanged
    {
        private readonly UpdateImpactResult model;
        private UpdateImpactViewFilter updateImpactViewFilter;

        public ComparisonViewModel(UpdateImpactResult model)
        {
            this.model = model;
            this.Files = new ObservableCollection<FileViewModel>();
            this.SetValues(model);
            this.FilesView = CollectionViewSource.GetDefaultView(this.Files);
            this.FilesView.Filter = this.FilterFiles;
        }

        public ICollectionView FilesView { get; }

        public UpdateImpactViewFilter UpdateImpactFilter
        {
            get => this.updateImpactViewFilter;
            set
            {
                if (!this.SetField(ref this.updateImpactViewFilter, value))
                {
                    return;
                }

                this.FilesView.Refresh();
            }
        }

        public long OldTotalSize { get; private set; }

        public long? OldCompressedSize { get; private set; }

        public long NewTotalSize { get; private set; }

        public long? NewCompressedSize { get; private set; }

        public long DeletedFilesSize { get; private set; }

        public int DeletedFilesCount { get; private set; }

        public long AddedFilesSize { get; private set; }

        public int AddedFilesCount { get; private set; }

        public long UnchangedFilesSize { get; private set; }

        public int UnchangedFilesCount { get; private set; }

        public long ChangedFilesOldSize { get; private set; }

        public long ChangedFilesNewSize { get; private set; }

        public int ChangedFileCount { get; private set; }

        public long NetSizeDifference { get; private set; }

        public long RequiredDownload { get; private set; }

        public PackageContentDetailsViewModel Content1 { get; private set; }

        public PackageContentDetailsViewModel Content2 { get; private set; }

        public ICollection<Block> NewBlocks { get; private set; }

        public ICollection<Block> OldBlocks { get; private set; }
        
        public long OldBlocksSize { get; private set; }

        public long NewBlocksSize { get; private set; }

        public ObservableCollection<FileViewModel> Files { get; }

        private Task<IList<FileViewModel>> LoadFiles(UpdateImpactResult updateImpact)
        {
            return Task.Run(() =>
            {
                var col = new List<FileViewModel>();

                var deleted = updateImpact.Comparison?.Package?.DeletedFiles?.Items ?? Enumerable.Empty<File>();
                var unchanged = updateImpact.Comparison?.Package?.UnchangedFiles?.Items ?? Enumerable.Empty<File>();
                var changed = updateImpact.Comparison?.Package?.ChangedFiles?.Items ?? Enumerable.Empty<ChangedFile>();
                var added = updateImpact.Comparison?.Package?.AddedFiles?.Items ?? Enumerable.Empty<File>();

                col.AddRange(deleted.Select(deletedFile => new FileViewModel(FileType.Deleted, deletedFile)));
                col.AddRange(added.Select(addedFile => new FileViewModel(FileType.Added, addedFile)));
                col.AddRange(unchanged.Select(unchangedFile => new FileViewModel(FileType.Unchanged, unchangedFile)));
                col.AddRange(changed.Select(changedFile => new FileViewModel(changedFile)));

                return (IList<FileViewModel>) col;
            });
        }

        private void LoadView(Task<IList<FileViewModel>> results)
        {
            if (results.IsFaulted || results.IsCanceled)
            {
                return;
            }

            this.Files.Clear();
            this.Files.AddRange(results.Result);
        }

        private void SetValues(UpdateImpactResult updateImpact)
        {
#pragma warning disable 4014
            var task = this.LoadFiles(updateImpact);
            task.ContinueWith(this.LoadView, CancellationToken.None, TaskContinuationOptions.AttachedToParent | TaskContinuationOptions.ExecuteSynchronously, TaskScheduler.Default);
#pragma warning restore 4014

            this.Content1 = new PackageContentDetailsViewModel(updateImpact.OldPackage.Manifest, updateImpact.OldPackage.Path);
            this.Content2 = new PackageContentDetailsViewModel(updateImpact.NewPackage.Manifest, updateImpact.NewPackage.Path);

            var comparePackage = updateImpact.Comparison;

            this.NewTotalSize = comparePackage.Package.Size;
            this.OldTotalSize = comparePackage.Package.Size - comparePackage.Package.SizeDifference;

            this.DeletedFilesSize = comparePackage.Package.DeletedSize;
            this.AddedFilesSize = comparePackage.Package.AddedSize;
            this.UnchangedFilesSize = comparePackage.Package.UnchangedFiles.Size;
            this.ChangedFilesNewSize = comparePackage.Package.Size - comparePackage.Package.AddedSize - comparePackage.Package.UnchangedFiles.Size;
            this.ChangedFilesOldSize = this.ChangedFilesNewSize - comparePackage.Package.ChangedFiles.SizeDifference;

            this.AddedFilesCount = comparePackage.Package.AddedFiles.Items.Count;
            this.DeletedFilesCount = comparePackage.Package.DeletedFiles.Items.Count;
            this.ChangedFileCount = comparePackage.Package.ChangedFiles.Items.Count;
            this.UnchangedFilesCount = comparePackage.Package.UnchangedFiles.Items.Count;

            this.NetSizeDifference = comparePackage.Package.SizeDifference;
            this.RequiredDownload = comparePackage.Package.UpdateImpact;

            this.OldCompressedSize = updateImpact.OldPackage?.Size;
            this.NewCompressedSize = updateImpact.NewPackage?.Size;

            this.OldBlocks = updateImpact.OldPackage.Blocks;
            this.NewBlocks = updateImpact.NewPackage.Blocks;

            this.OldBlocksSize = this.OldBlocks.Sum(b => b.Length);
            this.NewBlocksSize = this.NewBlocks.Sum(b => b.Length);
        }

        private bool FilterFiles(object obj)
        {
            if (this.UpdateImpactFilter == UpdateImpactViewFilter.None)
            {
                return true;
            }

            var current = (FileViewModel)obj;

            switch (this.UpdateImpactFilter)
            {
                case UpdateImpactViewFilter.OnlyChanged:
                    return current.Type != FileType.Unchanged;
                case UpdateImpactViewFilter.OnlyUpdateRelevant:
                    return current.UpdateImpact.HasValue && current.UpdateImpact > 0;
            }

            return true;
        }
    }
}
