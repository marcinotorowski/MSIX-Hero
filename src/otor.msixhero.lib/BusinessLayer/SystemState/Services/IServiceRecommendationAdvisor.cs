using System.Collections.Generic;
using System.ServiceProcess;
using otor.msixhero.lib.Domain.SystemState;
using otor.msixhero.lib.Domain.SystemState.Services;

namespace otor.msixhero.lib.BusinessLayer.SystemState.Services
{
    public interface IServiceRecommendationAdvisor
    {
        IEnumerable<IServiceRecommendation> Advise(AdvisorMode mode, params string[] ignoredServiceNames);
    }

    public class ServiceRecommendationAdvisor : IServiceRecommendationAdvisor
    {
        public IEnumerable<IServiceRecommendation> Advise(AdvisorMode mode, params string[] ignoredServiceNames)
        {
            // PLRestartMgrService","DPS","CscService","WSearch","wuauserv","CcmExec

            foreach (var serviceController in ServiceController.GetServices())
            {
                using (serviceController)
                {
                    switch (serviceController.ServiceName.ToLowerInvariant())
                    {
                        case "cscservice":
                            yield return new ServiceRecommendation(serviceController.DisplayName, "The Offline Files service performs maintenance activities on the Offline Files cache. It is recommended to disable this service during the repackaging of a software.", false, serviceController.Status != ServiceControllerStatus.Stopped);
                            break;

                        case "wsearch":
                            yield return new ServiceRecommendation(serviceController.DisplayName, "Indexing of files in the background can cause unnecessary noise. It is recommended to disable this service during the repackaging of a software. ", false, serviceController.Status != ServiceControllerStatus.Stopped);
                            break;

                        case "wuauserv":
                            yield return new ServiceRecommendation(serviceController.DisplayName, "Windows Update may periodically check and download a content which is not relevant for a repackaged app. It is recommended to disable this service during the repackaging of a software.", false, serviceController.Status != ServiceControllerStatus.Stopped);
                            break;

                        case "dps":
                            yield return new ServiceRecommendation(serviceController.DisplayName, "Disabling this service may improve performance and quality of repackaged packages.", false, serviceController.Status != ServiceControllerStatus.Stopped);
                            break;

                        case "msmpengs":
                            yield return new ServiceRecommendation(serviceController.DisplayName, "For the best repackaging experience, anti-virus software should be disabled when possible.", false, serviceController.Status != ServiceControllerStatus.Stopped);
                            break;
                    }
                }
            }
        }
    }
}
