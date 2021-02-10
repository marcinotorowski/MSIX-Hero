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
using Otor.MsixHero.Appx.Updates.Entities.Appx;

namespace Otor.MsixHero.App.Modules.Dialogs.Updates.UpdateImpact.ViewModel.Items
{
    public class ComparisonViewModel : NotifyPropertyChanged
    {
        private UpdateImpactViewFilter updateImpactViewFilter;

        public ComparisonViewModel(ComparisonResult model)
        {
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
        
        public PackageContentDetailsViewModel Content1 { get; private set; }

        public PackageContentDetailsViewModel Content2 { get; private set; }

        public ObservableCollection<FileViewModel> Files { get; }

        private Task<IList<FileViewModel>> LoadFiles(ComparisonResult updateImpact)
        {
            return Task.Run(() =>
            {
                var col = new List<FileViewModel>();

                var deleted = updateImpact.DeletedFiles?.Files ?? Enumerable.Empty<AppxFile>();
                var unchanged = updateImpact.UnchangedFiles?.Files ?? Enumerable.Empty<AppxFile>();
                var changedNew = updateImpact.ChangedFiles?.NewPackageFiles ?? Enumerable.Empty<AppxFile>();
                var added = updateImpact.AddedFiles?.Files ?? Enumerable.Empty<AppxFile>();

                col.AddRange(deleted.Select(deletedFile => new FileViewModel(FileType.Deleted, deletedFile)));
                col.AddRange(added.Select(addedFile => new FileViewModel(FileType.Added, addedFile)));
                col.AddRange(unchanged.Select(unchangedFile => new FileViewModel(FileType.Unchanged, unchangedFile)));
                col.AddRange(changedNew.Select(changedFile => new FileViewModel(changedFile)));

                return (IList<FileViewModel>)col;
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
            //var task = this.LoadFiles(comparePackage);
            //task.ContinueWith(this.LoadView, CancellationToken.None, TaskContinuationOptions.AttachedToParent | TaskContinuationOptions.ExecuteSynchronously, TaskScheduler.Default);
#pragma warning restore 4014

            // this.Content1 = new PackageContentDetailsViewModel(updateImpact.OldPackage.Manifest, updateImpact.OldPackage.Path);
            // this.Content2 = new PackageContentDetailsViewModel(updateImpact.NewPackage.Manifest, updateImpact.NewPackage.Path);

            this.ComparisonModel = comparePackage;
            this.OnPropertyChanged(nameof(ComparisonModel));
        }

        public ComparisonResult ComparisonModel { get; private set; }
        
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
