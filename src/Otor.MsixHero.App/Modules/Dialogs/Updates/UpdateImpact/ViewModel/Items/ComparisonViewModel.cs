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

using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows.Data;
using Otor.MsixHero.App.Mvvm;
using Otor.MsixHero.Appx.Updates.Entities;
using Otor.MsixHero.Appx.Updates.Entities.Appx;

namespace Otor.MsixHero.App.Modules.Dialogs.Updates.UpdateImpact.ViewModel.Items
{
    public class ComparisonViewModel : NotifyPropertyChanged
    {
        private UpdateImpactViewFilter updateImpactViewFilter = UpdateImpactViewFilter.OnlyUpdateRelevant;

        public ComparisonViewModel(UpdateImpactResults comparePackage)
        {
            this.ComparisonModel = comparePackage;

            var deleted = comparePackage.DeletedFiles?.Files ?? Enumerable.Empty<AppxFile>();
            var unchanged = comparePackage.UnchangedFiles?.Files ?? Enumerable.Empty<AppxFile>();
            var changedNew = comparePackage.ChangedFiles?.NewPackageFiles ?? Enumerable.Empty<AppxFile>();
            var added = comparePackage.AddedFiles?.Files ?? Enumerable.Empty<AppxFile>();

            this.Files = new ObservableCollection<FileViewModel>();
            this.Files.AddRange(deleted.Select(deletedFile => new FileViewModel(FileType.Deleted, deletedFile)));
            this.Files.AddRange(added.Select(addedFile => new FileViewModel(FileType.Added, addedFile)));
            this.Files.AddRange(unchanged.Select(unchangedFile => new FileViewModel(FileType.Unchanged, unchangedFile)));
            this.Files.AddRange(changedNew.Select(changedFile => new FileViewModel(changedFile)));
            
            this.FilesView = CollectionViewSource.GetDefaultView(this.Files);
            this.FilesView.Filter = this.FilterFiles;
            this.FilesView.SortDescriptions.Add(new SortDescription(nameof(FileViewModel.UpdateImpact), ListSortDirection.Descending));

            if (comparePackage.NewPackageDuplication?.Duplicates?.Any() == true)
            {
                this.Duplicates = new ObservableCollection<DuplicatedElementViewModel>(comparePackage.NewPackageDuplication.Duplicates.Select(d => new DuplicatedElementViewModel(d)));
            }
            else
            {
                this.Duplicates = new ObservableCollection<DuplicatedElementViewModel>();
            }

            this.DuplicatesView = CollectionViewSource.GetDefaultView(this.Duplicates);
            this.DuplicatesView.SortDescriptions.Add(new SortDescription(nameof(DuplicatedElementViewModel.DefaultSorting), ListSortDirection.Descending));
        }
        
        public ObservableCollection<DuplicatedElementViewModel> Duplicates { get; }
        
        public ICollectionView FilesView { get; }
        
        public ICollectionView DuplicatesView { get; }

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
        
        public ObservableCollection<FileViewModel> Files { get; }
        
        public UpdateImpactResults ComparisonModel { get; private set; }
        
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

        public void Export(string filePath)
        {
            this.ComparisonModel.Export(new FileInfo(filePath));
        }
    }
}
