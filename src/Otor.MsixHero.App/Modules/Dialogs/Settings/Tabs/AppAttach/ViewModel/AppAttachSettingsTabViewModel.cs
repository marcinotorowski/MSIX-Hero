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

using System.Threading.Tasks;
using Otor.MsixHero.App.Modules.Dialogs.Settings.Context;
using Otor.MsixHero.App.Mvvm.Changeable;
using Otor.MsixHero.Infrastructure.Configuration;
using Otor.MsixHero.Infrastructure.Services;
using Prism.Services.Dialogs;

namespace Otor.MsixHero.App.Modules.Dialogs.Settings.Tabs.AppAttach.ViewModel
{
    public class AppAttachSettingsTabViewModel : ChangeableContainer, ISettingsComponent
    {
        public AppAttachSettingsTabViewModel(
            IConfigurationService configurationService)
        {
            var config = configurationService.GetCurrentConfiguration();

            this.AddChildren(
                this.AppAttachGenerateScripts = new ChangeableProperty<bool>(config.AppAttach?.GenerateScripts == true),
                this.AppAttachExtractCertificate = new ChangeableProperty<bool>(config.AppAttach?.ExtractCertificate == true),
                this.AppAttachJunctionPoint = new ValidatedChangeableProperty<string>("Junction point", config.AppAttach?.JunctionPoint ?? "c:\\temp\\msix-app-attach", ValidatorFactory.ValidateNotEmptyField("Junction point")),
                this.AppAttachUseMsixMgr = new ChangeableProperty<bool>(config.AppAttach?.UseMsixMgrForVhdCreation ?? true));
        }

        public void Register(ISettingsContext context)
        {
            context.Register(this);
        }
        
        public ChangeableProperty<string> AppAttachJunctionPoint { get; }

        public ChangeableProperty<bool> AppAttachGenerateScripts { get; }

        public ChangeableProperty<bool> AppAttachExtractCertificate { get; }

        public ChangeableProperty<bool> AppAttachUseMsixMgr { get; }
        
        public bool CanCloseDialog()
        {
            return true;
        }

        public void OnDialogClosed()
        {
        }

        public void OnDialogOpened(IDialogParameters parameters)
        {
        }

        public bool CanSave()
        {
            return true;
        }
        
        public Task<bool> OnSaving(Configuration newConfiguration)
        {
            if (this.AppAttachExtractCertificate.IsTouched)
            {
                newConfiguration.AppAttach.ExtractCertificate = this.AppAttachExtractCertificate.CurrentValue;
            }

            if (this.AppAttachGenerateScripts.IsTouched)
            {
                newConfiguration.AppAttach.GenerateScripts = this.AppAttachGenerateScripts.CurrentValue;
            }

            if (this.AppAttachJunctionPoint.IsTouched)
            {
                newConfiguration.AppAttach.JunctionPoint = this.AppAttachJunctionPoint.CurrentValue;
            }

            if (this.AppAttachUseMsixMgr.IsTouched)
            {
                newConfiguration.AppAttach.UseMsixMgrForVhdCreation = this.AppAttachUseMsixMgr.CurrentValue;
            }

            return Task.FromResult(true);
        }
    }
}
