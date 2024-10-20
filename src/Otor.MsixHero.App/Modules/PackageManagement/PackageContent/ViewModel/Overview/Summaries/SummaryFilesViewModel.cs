// MSIX Hero
// Copyright (C) 2024 Marcin Otorowski
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
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using Otor.MsixHero.App.Modules.PackageManagement.PackageContent.Enums;
using Otor.MsixHero.App.Modules.PackageManagement.PackageContent.ViewModel.Common;
using Otor.MsixHero.App.Mvvm;
using Otor.MsixHero.Appx.Packaging;
using Otor.MsixHero.Appx.Reader.File;
using Otor.MsixHero.Appx.Reader.Manifest.Entities;
using Prism.Commands;

namespace Otor.MsixHero.App.Modules.PackageManagement.PackageContent.ViewModel.Overview.Summaries
{
    public class SummaryFilesViewModel : NotifyPropertyChanged, ILoadPackage
    {
        private static readonly TimeSpan MaxScanTime = TimeSpan.FromSeconds(5);
        private static readonly int MaxFileCount = 10000;
        private bool _isLoading;

        public SummaryFilesViewModel(IPackageContentItemNavigation navigation)
        {
            this.Details = new DelegateCommand(() => navigation.SetCurrentItem(PackageContentViewType.Files));
        }

        public ICommand Details { get; }

        public string FirstLine { get; private set; }

        public long FileSize { get; private set; }

        public string SecondLine { get; private set; }

        public Task LoadPackage(AppxPackage model, PackageEntry installationEntry, string filePath, CancellationToken cancellationToken)
        {
#pragma warning disable CS4014
            this.EstimateFilesCount(model, filePath, cancellationToken);
#pragma warning restore CS4014
            return Task.CompletedTask;
        }

        public bool IsLoading
        {
            get => this._isLoading;
            set => this.SetField(ref this._isLoading, value);
        }

        private async Task EstimateFilesCount(AppxPackage model, string filePath, CancellationToken cancellationToken)
        {
            this.IsLoading = true;
            try
            {
                this.IsEstimated = false;
                var fileReader = FileReaderFactory.CreateFileReader(filePath);

                var count = 0;
                var stopWatch = new Stopwatch();
                stopWatch.Start();
                this.Estimating.IsLoading = true;
                this.FirstLine = Resources.Localization.PackageExpert_Tabs_Files;
                this.SecondLine = Resources.Localization.Loading_PleaseWait;
                this.OnPropertyChanged(nameof(SecondLine));

                try
                {
                    var executables = model
                        .Applications
                        .Where(a => !string.IsNullOrEmpty(a?.Executable))
                        .OrderByDescending(a => a.Visible ? 1 : 0)
                        .ThenByDescending(a => a.Description?.Length ?? -1)
                        .SelectMany(a => a.Proxy?.Executable == null ? new[] { a.Executable } : new[] { a.Executable, a.Proxy.Executable })
                        .Distinct(StringComparer.OrdinalIgnoreCase)
                        .Take(2)
                        .ToArray();

                    long size = 0;
                    await foreach (var f in fileReader.EnumerateFiles(null, "*", SearchOption.AllDirectories, cancellationToken).ConfigureAwait(false))
                    {
                        count++;
                        size += f.Size;

                        if (stopWatch.Elapsed > MaxScanTime || count > MaxFileCount)
                        {
                            this.IsEstimated = true;
                            break;
                        }
                    }

                    this.FileSize = size;
                    this.FilesCount = count;
                    this.OtherFilesCount = count;

                    if (count == 0)
                    {
                        this.FirstLine = Resources.Localization.PackageExpert_Tabs_Files;
                    }
                    else if (this.IsEstimated)
                    {
                        this.FirstLine = Resources.Localization.PackageExpert_Tabs_Files + ": " + string.Format(Resources.Localization.PackageExpert_Files_Summary_Estimated, count, Convert(size));
                    }
                    else
                    {
                        this.FirstLine = Resources.Localization.PackageExpert_Tabs_Files + ": " + string.Format(Resources.Localization.PackageExpert_Files_Summary_Exact, count, Convert(size));
                    }

                    switch (executables.Length)
                    {
                        case 0:
                            if (this.OtherFilesCount == 0)
                            {
                                this.SecondLine = Resources.Localization.PackageExpert_Files_Summary_None;
                            }
                            else
                            {
                                this.SecondLine =
                                    this.IsEstimated
                                        ? string.Format(Resources.Localization.PackageExpert_Files_Summary_FilesGenericPlus, this.OtherFilesCount)
                                        : string.Format(Resources.Localization.PackageExpert_Files_Summary_FilesGeneric, this.OtherFilesCount);
                            }
                            break;
                        case 1:
                            this.OtherFilesCount--;
                            this.SecondLine =
                                this.IsEstimated
                                    ? string.Format(Resources.Localization.PackageExpert_Files_Summary_FilesExe1Plus, Path.GetFileName(executables[0]), this.OtherFilesCount)
                                    : string.Format(Resources.Localization.PackageExpert_Files_Summary_FilesExe1, Path.GetFileName(executables[0]), this.OtherFilesCount);
                            break;

                        default:
                            this.OtherFilesCount -= 2;
                            this.SecondLine =
                                this.IsEstimated
                                    ? string.Format(Resources.Localization.PackageExpert_Files_Summary_FilesExe2Plus, Path.GetFileName(executables[0]), Path.GetFileName(executables[1]), this.OtherFilesCount)
                                    : string.Format(Resources.Localization.PackageExpert_Files_Summary_FilesExe2, Path.GetFileName(executables[0]), Path.GetFileName(executables[1]), this.OtherFilesCount);
                            break;
                    }
                }
                catch (OperationCanceledException)
                {
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
            finally
            {
                this.IsLoading = false;
            }
        }

        public int FilesCount { get; private set; }

        public int OtherFilesCount { get; private set; }

        public bool IsEstimated { get; private set; }

        public ProgressProperty Estimating { get; } = new();

        private static string Convert(long value)
        {
            var units = new[] { "B", "KB", "MB", "GB", "TB" };

            var hasMinus = value < 0;

            var doubleValue = (double)Math.Abs(value);
            var index = 0;
            while (doubleValue > 1024 && index < units.Length)
            {
                doubleValue /= 1024.0;
                index++;
            }

            if (index == 0)
            {
                return value + " " + units[index];
            }

            return (hasMinus ? "-" : string.Empty) + (Math.Round(doubleValue * 10, 0) / 10.0).ToString("0.0") + " " + units[index];
        }
    }
}
