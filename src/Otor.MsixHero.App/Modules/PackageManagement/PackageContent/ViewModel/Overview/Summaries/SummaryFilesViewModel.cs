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

using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Otor.MsixHero.App.Helpers;
using Otor.MsixHero.App.Modules.PackageManagement.PackageContent.Enums;
using Otor.MsixHero.App.Modules.PackageManagement.PackageContent.ViewModel.Common;
using Otor.MsixHero.App.Mvvm;
using Otor.MsixHero.Appx.Packaging.Manifest.Entities;
using Otor.MsixHero.Appx.Packaging.Manifest.FileReaders;
using Prism.Commands;

namespace Otor.MsixHero.App.Modules.PackageManagement.PackageContent.ViewModel.Overview.Summaries
{
    public class SummaryFilesViewModel : NotifyPropertyChanged, ILoadPackage
    {
        private static readonly TimeSpan MaxScanTime = TimeSpan.FromSeconds(5);
        private static readonly int MaxFileCount = 10000;
        
        public SummaryFilesViewModel(IPackageContentItemNavigation navigation)
        {
            this.Details = new DelegateCommand(() => navigation.SetCurrentItem(PackageContentViewType.Files));
        }

        public ICommand Details { get; }

        public string FirstLine { get; private set; }

        public string SecondLine { get; private set; }

        public Task LoadPackage(AppxPackage model, string filePath)
        {
            this.EstimateFilesCount(model, filePath);
            return Task.CompletedTask;
        }

        private async void EstimateFilesCount(AppxPackage model, string filePath)
        {
            var fileReader = FileReaderFactory.CreateFileReader(filePath);

            var count = 0;
            var stopWatch = new Stopwatch();
            stopWatch.Start();
            this.Estimating.IsLoading = true;
            this.FirstLine = "Files";
            this.SecondLine = "Calculating...";
            this.OnPropertyChanged(nameof(SecondLine));

            try
            {
                var executables = model
                    .Applications
                    .Where(a => !string.IsNullOrEmpty(a?.Executable))
                    .OrderByDescending(a => a.Visible ? 1 : 0)
                    .ThenByDescending(a => a.Description?.Length ?? -1)
                    .SelectMany(a => a.Psf?.Executable == null ? new[] { a.Executable } : new[] { a.Executable, a.Psf.Executable })
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .Take(2)
                    .ToArray();

                await foreach (var _ in fileReader.EnumerateFiles(null, "*", SearchOption.AllDirectories).ConfigureAwait(false))
                {
                    count++;
                    if (stopWatch.Elapsed > MaxScanTime || count > MaxFileCount)
                    {
                        this.IsEstimated = true;
                        break;
                    }
                }

                this.FilesCount = count;
                this.OtherFilesCount = count;

                if (count == 0)
                {
                    this.FirstLine = "Files";
                }
                else if (this.IsEstimated)
                {
                    this.FirstLine = "Files: more than **" + count + "**";
                }
                else
                {
                    this.FirstLine = "Files: **" + count + "**";
                }
                
                switch (executables.Length)
                {
                    case 0:
                        if (this.OtherFilesCount == 0)
                        {
                            this.SecondLine = "This package contains no files.";
                        }
                        else
                        {
                            this.SecondLine =
                                this.IsEstimated
                                    ? string.Format("{0}+ files", this.OtherFilesCount)
                                    : string.Format("{0} files", this.OtherFilesCount);
                        }
                        break;
                    case 1:
                        this.OtherFilesCount--;
                        this.SecondLine =
                            this.IsEstimated
                                ? string.Format("{0} and {1}+ other files", Path.GetFileName(executables[0]), this.OtherFilesCount)
                                : string.Format("{0} and {1} other files", Path.GetFileName(executables[0]), this.OtherFilesCount);
                        break;

                    default:
                        this.OtherFilesCount -= 2;
                        this.SecondLine =
                            this.IsEstimated
                                ? string.Format("{0}, {1} and {2}+ other files", Path.GetFileName(executables[0]), Path.GetFileName(executables[1]), this.OtherFilesCount)
                                : string.Format("{0}, {1} and {2} other files", Path.GetFileName(executables[0]), Path.GetFileName(executables[1]), this.OtherFilesCount);
                        break;
                }
            }
            finally
            {
                stopWatch.Stop();
                this.Estimating.IsLoading = false;
                this.OnPropertyChanged(nameof(IsEstimated));
                this.OnPropertyChanged(nameof(FirstLine));
                this.OnPropertyChanged(nameof(SecondLine));
                this.OnPropertyChanged(nameof(FilesCount));
                this.OnPropertyChanged(nameof(OtherFilesCount));
            }
        }

        public int FilesCount { get; private set; }

        public int OtherFilesCount { get; private set; }

        public bool IsEstimated { get; private set; }

        public ProgressProperty Estimating { get; } = new ProgressProperty();
    }
}
