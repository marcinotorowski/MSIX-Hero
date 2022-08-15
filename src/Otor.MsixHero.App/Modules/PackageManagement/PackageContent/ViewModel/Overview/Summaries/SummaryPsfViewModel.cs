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

using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using Otor.MsixHero.App.Modules.PackageManagement.PackageContent.Enums;
using Otor.MsixHero.App.Modules.PackageManagement.PackageContent.ViewModel.Common;
using Otor.MsixHero.App.Mvvm;
using Otor.MsixHero.Appx.Packaging.Manifest.Entities;
using Prism.Commands;

namespace Otor.MsixHero.App.Modules.PackageManagement.PackageContent.ViewModel.Overview.Summaries
{
    public class SummaryPsfViewModel : NotifyPropertyChanged, ILoadPackage
    {
        public SummaryPsfViewModel(IPackageContentItemNavigation navigation)
        {
            this.Details = new DelegateCommand(() => navigation.SetCurrentItem(PackageContentViewType.Psf));
        }

        public ICommand Details { get; }

        public Task LoadPackage(AppxPackage model, string filePath, CancellationToken cancellationToken)
        {
            this.HasPsf = model.Applications.Any(a => a.Psf != null);
            
            var hasRedirections = model.Applications.Select(a => a.Psf).Any(psf => psf?.FileRedirections?.Any() == true);
            var hasTracing = model.Applications.Select(a => a.Psf).Any(psf => psf?.Tracing != null);
            var hasOtherFixUps = model.Applications.Select(a => a.Psf).Any(psf => psf?.OtherFixups?.Any() == true);
            var hasElectron = model.Applications.Select(a => a.Psf).Any(psf => psf?.Electron != null);

            var hasScripts = model.Applications.Select(a => a.Psf).Any(psf => psf?.Scripts?.Any() == true);

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

        public string SecondLine { get; private set; }

        public bool HasPsf { get; private set; }
    }
}
