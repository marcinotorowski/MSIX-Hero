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
using Otor.MsixHero.App.Modules.PackageManagement.PackageContent.ViewModel.Capabilities.Items;
using Otor.MsixHero.App.Modules.PackageManagement.PackageContent.ViewModel.Common;
using Otor.MsixHero.App.Mvvm;
using Otor.MsixHero.Appx.Packaging;
using Otor.MsixHero.Appx.Reader.Manifest.Entities;
using Prism.Commands;

namespace Otor.MsixHero.App.Modules.PackageManagement.PackageContent.ViewModel.Overview.Summaries
{
    public class SummaryCapabilitiesViewModel : NotifyPropertyChanged, ILoadPackage
    {
        private bool _isLoading;

        public SummaryCapabilitiesViewModel(IPackageContentItemNavigation navigation)
        {
            this.Details = new DelegateCommand(() => navigation.SetCurrentItem(PackageContentViewType.Capabilities));
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

                var stringBuilder = new StringBuilder();

                this.Count = 0;
                foreach (var capability in model.Capabilities.Where(c => c.Name == "runFullTrust" || c.Name == "allowElevation"))
                {
                    if (this.Count >= 2)
                    {
                        break;
                    }

                    this.Count++;

                    if (stringBuilder.Length != 0)
                    {
                        stringBuilder.Append(", ");
                    }

                    switch (capability.Name)
                    {
                        case "runFullTrust":
                            stringBuilder.Append(Resources.Localization.PackageExpert_Capabilities_Summary_Trust);
                            break;
                        case "allowElevation":
                            stringBuilder.Append(Resources.Localization.PackageExpert_Capabilities_Summary_Elevation);
                            break;
                    }
                }

                foreach (var capability in model.Capabilities.Where(c => c.Name != "runFullTrust" && c.Name != "allowElevation"))
                {
                    if (this.Count >= 2)
                    {
                        break;
                    }

                    this.Count++;

                    if (stringBuilder.Length != 0)
                    {
                        stringBuilder.Append(", ");
                    }

                    stringBuilder.Append(CapabilityTranslationProvider.ToDisplayName(capability.Name));
                }
                
                if (model.Capabilities.Count - this.Count == 1)
                {
                    stringBuilder.Append(' ');
                    stringBuilder.Append(Resources.Localization.PackageExpert_Capabilities_Summary_OneAnother);
                }
                else if (model.Capabilities.Count - this.Count > 0)
                {
                    stringBuilder.Append(' ');
                    stringBuilder.AppendFormat(Resources.Localization.PackageExpert_Capabilities_Summary_XOther, model.Capabilities.Count - this.Count);
                }

                this.Count = model.Capabilities.Count;
                if (stringBuilder.Length == 0)
                {
                    this.Summary = Resources.Localization.PackageExpert_Capabilities_Summary_None;
                }
                else
                {
                    this.Summary = stringBuilder.ToString(0, 1).ToUpper() + stringBuilder.ToString(1, stringBuilder.Length - 1);
                }
                
                this.OnPropertyChanged(null);

                return Task.CompletedTask;
            }
            finally
            {
                this.IsLoading = false;
            }
        }
        
        public int Count { get; private set; }
        
        public string Summary { get; private set; }
    }
}
