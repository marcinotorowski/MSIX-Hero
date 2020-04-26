using System;
using System.Threading.Tasks;
using otor.msixhero.lib.BusinessLayer.SystemState.Services;
using otor.msixhero.lib.Domain.SystemState.Services;
using otor.msixhero.lib.Infrastructure;
using otor.msixhero.ui.ViewModel;

namespace otor.msixhero.ui.Modules.SystemStatus.ViewModel.Repackaging
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
        }

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
                    this.interactionService.ShowError("The action could not be executed.", InteractionResult.OK);
                }

                this.IsRunning = desiredState;
                await this.parent.Refresh().ConfigureAwait(false);
            }
            catch (Exception e)
            {
                this.interactionService.ShowError("The action could not be executed.", e, InteractionResult.OK);
            }
            finally
            {
                this.IsFixing = true;
            }
        }
    }
}
