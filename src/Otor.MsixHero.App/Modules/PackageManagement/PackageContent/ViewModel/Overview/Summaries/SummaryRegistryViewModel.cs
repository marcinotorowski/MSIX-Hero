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
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using Otor.MsixHero.App.Modules.PackageManagement.PackageContent.Enums;
using Otor.MsixHero.App.Modules.PackageManagement.PackageContent.ViewModel.Common;
using Otor.MsixHero.App.Mvvm;
using Otor.MsixHero.Appx.Packaging;
using Otor.MsixHero.Appx.Packaging.Manifest.Entities;
using Otor.MsixHero.Appx.Packaging.Registry;
using Otor.MsixHero.Appx.Reader;
using Prism.Commands;

namespace Otor.MsixHero.App.Modules.PackageManagement.PackageContent.ViewModel.Overview.Summaries
{
    public class SummaryRegistryViewModel : NotifyPropertyChanged, ILoadPackage
    {
        private bool _isLoading;

        public SummaryRegistryViewModel(IPackageContentItemNavigation navigation)
        {
            this.Details = new DelegateCommand(() => navigation.SetCurrentItem(PackageContentViewType.Registry));
        }

        public bool IsLoading
        {
            get => this._isLoading;
            private set => this.SetField(ref this._isLoading, value);
        }

        public ICommand Details { get; }

        public string SecondLine { get; private set; }

        public Task LoadPackage(AppxPackage model, PackageEntry installationEntry, string filePath, CancellationToken cancellationToken)
        {
            this.IsLoading = true;

            var fileReader = FileReaderFactory.CreateFileReader(filePath);
            this.HasRegistry = fileReader.FileExists("Registry.dat");
            this.OnPropertyChanged(nameof(HasRegistry));

            if (this.HasRegistry)
            {
                this.EstimateRegistryCount(fileReader, cancellationToken).ContinueWith(t =>
                {
                    if (t.IsFaulted)
                    {
                        this.HasRegistry = false;
                    }

                    this.IsLoading = false;
                }, 
                CancellationToken.None);
            }
            else
            {
                this.IsLoading = false;
            }

            return Task.CompletedTask;
        }

        public bool HasRegistry { get; private set; }


        private async Task EstimateRegistryCount(IAppxFileReader fileReader, CancellationToken cancellationToken)
        {
            await using var f = fileReader.GetFile("Registry.dat");
            using var appxRegistryReader = new AppxRegistryReader(f);
            var hasMachine = false;
            var hasUser = false;

            var stopWatch = new Stopwatch();
            stopWatch.Start();
            this.Estimating.IsLoading = true;
            this.SecondLine = Resources.Localization.Packages_Calculating;
            this.OnPropertyChanged(nameof(SecondLine));

            try
            {
                await foreach (var root in appxRegistryReader.EnumerateKeys(AppxRegistryRoots.Root, cancellationToken)
                                   .ConfigureAwait(false))
                {
                    if (string.Equals(AppxRegistryRoots.HKLM.TrimEnd('\\'), root.Path,
                            StringComparison.OrdinalIgnoreCase))
                    {
                        hasMachine = true;
                    }

                    if (string.Equals(AppxRegistryRoots.HKCU.TrimEnd('\\'), root.Path,
                            StringComparison.OrdinalIgnoreCase))
                    {
                        hasUser = true;
                    }
                }

                if (hasMachine && hasUser)
                {
                    this.SecondLine = Resources.Localization.PackageExpert_Registry_SummaryBuilder_General;
                }
                else if (hasMachine)
                {
                    var findKey = await appxRegistryReader
                        .EnumerateKeys(AppxRegistryRoots.HKLM + "Software", cancellationToken).FirstOrDefaultAsync(cancellationToken: cancellationToken)
                        .ConfigureAwait(false);
                    if (findKey.Path != null)
                    {
                        this.SecondLine = string.Format(Resources.Localization.PackageExpert_Registry_SummaryBuilder_HKLM_Other, findKey.Path.Substring(AppxRegistryRoots.HKLM.Length));
                    }
                    else
                    {
                        this.SecondLine = Resources.Localization.PackageExpert_Registry_SummaryBuilder_Machine;
                    }
                }
                else if (hasUser)
                {
                    var findKey = await appxRegistryReader
                        .EnumerateKeys(AppxRegistryRoots.HKCU + "Software", cancellationToken).FirstOrDefaultAsync(cancellationToken: cancellationToken)
                        .ConfigureAwait(false);

                    if (findKey.Path != null)
                    {
                        this.SecondLine = string.Format(Resources.Localization.PackageExpert_Registry_SummaryBuilder_HKCU_Other, findKey.Path.Substring(AppxRegistryRoots.HKCU.Length));
                    }
                    else
                    {
                        this.SecondLine = Resources.Localization.PackageExpert_Registry_SummaryBuilder_User;
                    }
                }
                else
                {
                    this.SecondLine = Resources.Localization.PackageExpert_Registry_SummaryBuilder_Unknown;
                }
            }
            catch (OperationCanceledException)
            {
                this.SecondLine = Resources.Localization.PackageExpert_Registry_SummaryBuilder_Unknown;
            }
            finally
            {
                stopWatch.Stop();
                this.Estimating.IsLoading = false;
                this.OnPropertyChanged(nameof(SecondLine));
            }
        }
        
        public ProgressProperty Estimating { get; } = new ProgressProperty();
    }
}
