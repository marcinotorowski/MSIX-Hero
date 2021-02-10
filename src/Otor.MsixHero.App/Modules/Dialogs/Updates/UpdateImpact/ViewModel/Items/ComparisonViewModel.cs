// MSIX Hero
// Copyright (C) 2021 Marcin Otorowski
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

namespace Otor.MsixHero.App.Modules.Dialogs.Updates.UpdateImpact.ViewModel.Items
{
    public class ComparisonViewModel : NotifyPropertyChanged
    {
        private readonly ComparisonResult model;
        private UpdateImpactViewFilter updateImpactViewFilter;

        public ComparisonViewModel(ComparisonResult model)
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

        public ICollection<ComparedChart> NewBlocks { get; private set; }

        public ICollection<ComparedChart> OldBlocks { get; private set; }

        public ICollection<ComparedChart> NewFiles { get; private set; }

        public ICollection<ComparedChart> OldFiles { get; private set; }
        
        public long OldBlocksSize { get; private set; }

        public long NewBlocksSize { get; private set; }
        
        public long OldFilesSize { get; private set; }

        public long NewFilesSize { get; private set; }

        public ObservableCollection<FileViewModel> Files { get; }

        private Task<IList<FileViewModel>> LoadFiles(ComparisonResult updateImpact)
        {
            return Task.Run(() =>
            {
                var col = new List<FileViewModel>();

                var deleted = updateImpact.DeletedFiles?.Files ?? Enumerable.Empty<ComparedFile>();
                var unchanged = updateImpact.UnchangedFiles?.Files ?? Enumerable.Empty<ComparedFile>();
                var changed = updateImpact.ChangedFiles?.Files ?? Enumerable.Empty<ComparedFile>();
                var added = updateImpact.NewFiles?.Files ?? Enumerable.Empty<ComparedFile>();

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

        private void SetValues(ComparisonResult comparePackage)
        {
#pragma warning disable 4014
            var task = this.LoadFiles(comparePackage);
            task.ContinueWith(this.LoadView, CancellationToken.None, TaskContinuationOptions.AttachedToParent | TaskContinuationOptions.ExecuteSynchronously, TaskScheduler.Default);
#pragma warning restore 4014

            // this.Content1 = new PackageContentDetailsViewModel(updateImpact.OldPackage.Manifest, updateImpact.OldPackage.Path);
            // this.Content2 = new PackageContentDetailsViewModel(updateImpact.NewPackage.Manifest, updateImpact.NewPackage.Path);
            
            this.NewTotalSize = comparePackage.TargetTotalSize;
            this.OldTotalSize = comparePackage.BaseTotalSize;

            this.DeletedFilesSize = comparePackage.DeletedFiles.BaseTotalSize;
            this.AddedFilesSize = comparePackage.NewFiles.TargetTotalSize;
            this.UnchangedFilesSize = comparePackage.UnchangedFiles.TargetTotalSize;
            this.ChangedFilesNewSize = comparePackage.ChangedFiles.TargetTotalSize;
            this.ChangedFilesOldSize = comparePackage.ChangedFiles.BaseTotalSize;

            this.AddedFilesCount = comparePackage.NewFiles.Files.Count;
            this.DeletedFilesCount = comparePackage.DeletedFiles.Files.Count;
            this.ChangedFileCount = comparePackage.ChangedFiles.Files.Count;
            this.UnchangedFilesCount = comparePackage.UnchangedFiles.Files.Count;

            this.NetSizeDifference = comparePackage.SizeDifference;
            this.RequiredDownload = comparePackage.UpdateImpact;

            this.OldCompressedSize = comparePackage.BaseTotalCompressedSize;
            this.NewCompressedSize = comparePackage.TargetTotalCompressedSize;

            this.OldBlocks = comparePackage.BaseChart.Blocks;
            this.NewBlocks = comparePackage.TargetChart.Blocks;

            this.OldFiles = comparePackage.BaseChart.Files;
            this.NewFiles = comparePackage.TargetChart.Files;

            this.OldBlocksSize = comparePackage.DeletedFiles.Files.SelectMany(f => f.Blocks).Where(b => b.Status == ComparisonStatus.Old).Sum(b => b.CompressedSize) +
                                 comparePackage.ChangedFiles.Files.SelectMany(f => f.Blocks).Where(b => b.Status == ComparisonStatus.Old).Sum(b => b.CompressedSize);
            
            this.NewBlocksSize = comparePackage.NewFiles.Files.SelectMany(f => f.Blocks).Where(b => b.Status == ComparisonStatus.New).Sum(b => b.CompressedSize) +
                                 comparePackage.ChangedFiles.Files.SelectMany(f => f.Blocks).Where(b => b.Status == ComparisonStatus.New).Sum(b => b.CompressedSize);

            this.OldFilesSize = comparePackage.BaseTotalSize;
            this.NewFilesSize = comparePackage.TargetTotalSize;
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
