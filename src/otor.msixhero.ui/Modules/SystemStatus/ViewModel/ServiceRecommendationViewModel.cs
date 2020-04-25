using System;
using System.Collections.Generic;
using System.Text;
using otor.msixhero.lib.Domain.SystemState.Services;
using otor.msixhero.ui.ViewModel;

namespace otor.msixhero.ui.Modules.SystemStatus.ViewModel
{
    public class ServiceRecommendationViewModel : NotifyPropertyChanged
    {
        public ServiceRecommendationViewModel(IServiceRecommendation model)
        {
            this.DisplayName = model.Name;
            this.IsRunning = model.Actual;
            this.ShouldRun = model.ExpectedToRun;
            this.Recommendation = model.ActionPrompt;
        }

        public string DisplayName { get; }

        public string Recommendation { get; }

        public bool IsRunning { get; }

        public bool ShouldRun { get; }
    }
}
