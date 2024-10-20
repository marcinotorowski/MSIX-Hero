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

using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using Otor.MsixHero.App.Modules.PackageManagement.PackageContent.Enums;
using Otor.MsixHero.App.Modules.PackageManagement.PackageContent.ViewModel.Common;
using Otor.MsixHero.App.Mvvm;
using Otor.MsixHero.Appx.Packaging;
using Otor.MsixHero.Appx.Reader.Manifest.Entities;
using Otor.MsixHero.Appx.Reader.Psf.Entities.Descriptor;
using Prism.Commands;

namespace Otor.MsixHero.App.Modules.PackageManagement.PackageContent.ViewModel.Overview.Summaries
{
    public class SummaryPsfViewModel : NotifyPropertyChanged, ILoadPackage
    {
        private bool _isLoading;

        public SummaryPsfViewModel(IPackageContentItemNavigation navigation)
        {
            this.Details = new DelegateCommand(() => navigation.SetCurrentItem(PackageContentViewType.Psf));
        }

        public bool IsLoading
        {
            get => this._isLoading;
            private set => this.SetField(ref this._isLoading, value);
        }

        public ICommand Details { get; }

        public Task LoadPackage(AppxPackage model, PackageEntry installEntry, string filePath, CancellationToken cancellationToken)
        {
            try
            {
                this.IsLoading = true;

                this.HasPsf = model.Applications.Any(a => a.Proxy != null && a.Proxy.Type == ApplicationProxyType.PackageSupportFramework);

                var psfDescriptors = model.Applications.Select(a => a.Proxy).OfType<PsfApplicationProxy>();

                // ReSharper disable PossibleMultipleEnumeration
                var hasRedirections = psfDescriptors.Any(p => p.FileRedirections?.Any() == true);
                var hasTracing = psfDescriptors.Any(p => p.Tracing != null);
                var hasOtherFixUps = psfDescriptors.Any(p => p.OtherFixups?.Any() == true);
                var hasElectron = psfDescriptors.Any(p => p.Electron != null);
                var hasScripts = psfDescriptors.Any(p => p.Scripts?.Any() == true);
                // ReSharper restore PossibleMultipleEnumeration

                var secondLine = new StringBuilder(Resources.Localization.PackageExpert_PSF_SummaryBuilder_Header);
                secondLine.Append(" ");
                var count = 0;
                if (hasRedirections)
                {
                    // ReSharper disable once ConditionIsAlwaysTrueOrFalse
                    if (count++ > 0)
                    {
                        secondLine.Append(", ");
                    }

                    secondLine.Append(Resources.Localization.PackageExpert_PSF_SummaryBuilder_Redirections);
                }

                if (hasScripts)
                {
                    if (count++ > 0)
                    {
                        secondLine.Append(", ");
                    }

                    secondLine.Append(Resources.Localization.PackageExpert_PSF_SummaryBuilder_Scripts);
                }

                if (hasTracing)
                {
                    if (count++ > 0)
                    {
                        secondLine.Append(", ");
                    }

                    secondLine.Append(Resources.Localization.PackageExpert_PSF_SummaryBuilder_Tracing);
                }

                if (hasElectron)
                {
                    if (count++ > 0)
                    {
                        secondLine.Append(", ");
                    }

                    secondLine.Append(Resources.Localization.PackageExpert_PSF_SummaryBuilder_Electron);
                }

                if (hasOtherFixUps)
                {
                    if (count++ > 0)
                    {
                        secondLine.Append(", ");
                    }

                    secondLine.Append(Resources.Localization.PackageExpert_PSF_SummaryBuilder_Custom);
                }

                if (count == 0)
                {
                    this.SecondLine = Resources.Localization.PackageExpert_PSF_Summary_Custom;
                }
                else
                {
                    this.SecondLine = secondLine.ToString();
                }

                this.OnPropertyChanged(null);
                return Task.CompletedTask;
            }
            finally
            {
                this.IsLoading = false;  
            }
        }

        public string SecondLine { get; private set; }

        public bool HasPsf { get; private set; }
    }
}
