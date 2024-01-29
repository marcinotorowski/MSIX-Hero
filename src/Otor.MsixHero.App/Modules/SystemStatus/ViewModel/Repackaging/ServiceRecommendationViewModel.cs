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
using System.Threading.Tasks;
using Otor.MsixHero.App.Mvvm;
using Otor.MsixHero.Appx.Diagnostic.Recommendations;
using Otor.MsixHero.Appx.Diagnostic.Recommendations.Entities;
using Otor.MsixHero.Appx.Diagnostic.Recommendations.Enums;
using Otor.MsixHero.Infrastructure.Services;

namespace Otor.MsixHero.App.Modules.SystemStatus.ViewModel.Repackaging
{
    public class ServiceRecommendationViewModel : NotifyPropertyChanged
    {
        private bool isFixing;
        private readonly IServiceRecommendation model;
        private readonly IInteractionService interactionService;
        private readonly RepackagingRecommendationViewModel parent;
        private readonly IServiceRecommendationAdvisor advisor;
        private bool isRunning;

        public ServiceRecommendationViewModel(
            RepackagingRecommendationViewModel parent,
            IServiceRecommendationAdvisor advisor, 
            IServiceRecommendation model, 
            IInteractionService interactionService)
        {
            this.parent = parent;
            this.advisor = advisor;
            this.model = model;
            this.interactionService = interactionService;
            this.DisplayName = model.DisplayName;
            this.ServiceName = model.ServiceName;
            this.IsRunning = model.Actual;
            this.ShouldRun = model.ExpectedToRun;
            this.Recommendation = model.ActionPrompt;
            this.Type = model.Type;
        }

        public ServiceRecommendationType Type { get; }

        public string ServiceName { get; }

        public string DisplayName { get; }

        public string Recommendation { get; }

        public bool IsRunning
        {
            get => this.isRunning;
            set => this.SetField(ref this.isRunning, value);
        }

        public bool ShouldRun { get; }

        public bool IsFixing
        {
            get => this.isFixing;
            set => this.SetField(ref this.isFixing, value);
        }

        public void Start()
        {
#pragma warning disable 4014
            this.Fix(!this.model.ExpectedToRun);
#pragma warning restore 4014
        }

        public void Stop()
        {
#pragma warning disable 4014
            this.Fix(this.model.ExpectedToRun);
#pragma warning restore 4014
        }

        private async Task Fix(bool desiredState)
        {
            this.IsFixing = true;
            
            try
            {
                bool result;
                if (desiredState == this.model.ExpectedToRun)
                {
                    result = await advisor.Fix(this.model).ConfigureAwait(false);
                }
                else
                {
                    result = await advisor.Revert(this.model).ConfigureAwait(false);
                }
                
                if (!result)
                {
                    this.interactionService.ShowError(Resources.Localization.System_Action_Error, InteractionResult.OK);
                }

                this.IsRunning = desiredState;
                await this.parent.Refresh().ConfigureAwait(false);
            }
            catch (Exception e)
            {
                this.interactionService.ShowError(Resources.Localization.System_Action_Error, e, InteractionResult.OK);
            }
            finally
            {
                this.IsFixing = true;
            }
        }
    }
}
