using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.ServiceProcess;
using System.Threading.Tasks;
using otor.msixhero.lib.Domain.SystemState;
using otor.msixhero.lib.Domain.SystemState.Services;

namespace otor.msixhero.lib.BusinessLayer.SystemState.Services
{
    public class ServiceRecommendationAdvisor : IServiceRecommendationAdvisor
    {
        public Task<bool> Fix(IServiceRecommendation recommendation)
        {
            return this.ChangeStatus(recommendation.ServiceName, recommendation.ExpectedToRun);
        }

        public Task<bool> Revert(IServiceRecommendation recommendation)
        {
            return this.ChangeStatus(recommendation.ServiceName, !recommendation.ExpectedToRun);
        }

        private Task<bool> ChangeStatus(string name, bool expectedRunState)
        {
            return Task.Run(() =>
            {
                var system32 = Environment.GetFolderPath(Environment.SpecialFolder.System);

                if (expectedRunState && this.IsServiceRunning(name))
                {
                    return true;
                }

                if (!expectedRunState && this.IsServiceDisabled(name))
                {
                    return true;
                }
                
                var psi = new ProcessStartInfo()
                {
                    Verb = "runas",
                    UseShellExecute = true,
#if !DEBUG
                    WindowStyle = ProcessWindowStyle.Hidden,
                    CreateNoWindow = true
#endif
                };

                if (expectedRunState)
                {
                    if (!this.IsServiceDisabled(name))
                    {
                        // Expected: ACTIVE
                        // Actual: Not running, not disabled
                        // Action: Start
                        psi.FileName = Path.Combine(system32, "sc.exe");
                        psi.Arguments = "start " + name;
                    }
                    else
                    {
                        // Expected: ACTIVE
                        // Actual: Not running, disabled
                        // Action: Enable and Start
                        psi.FileName = Path.Combine(system32, "cmd.exe");
                        psi.Arguments = "/c \"sc.exe config " + name + " start= auto && sc.exe start " + name + "\"";
                    }
                }
                else
                {
                    // Expected: DISABLED
                    // Actual: Not running
                    // Action: Stop and disable
                    psi.FileName = Path.Combine(system32, "cmd.exe");
                    psi.Arguments = "/c \"sc.exe stop " + name + " && sc.exe config " + name + " start= disabled\"";
                }

                var p = Process.Start(psi);
                if (p == null)
                {
                    return false;
                }

                p.WaitForExit();
                if (p.ExitCode != 0)
                {
                    return false;
                }

                if (expectedRunState && this.IsServiceRunning(name))
                {
                    return true;
                }

                if (!expectedRunState && this.IsServiceDisabled(name))
                {
                    return true;
                }

                return false;
            });
        }

        private bool IsServiceRunning(string serviceName)
        {
            using var serviceController = new ServiceController(serviceName);
            return serviceController.Status == ServiceControllerStatus.Running || serviceController.Status == ServiceControllerStatus.StartPending;
        }

        private bool IsServiceDisabled(string serviceName)
        {
            using var serviceController = new ServiceController(serviceName);
            return serviceController.StartType == ServiceStartMode.Disabled;
        }

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
                            yield return new ServiceRecommendation(serviceController.ServiceName, serviceController.DisplayName, "The Offline Files service performs maintenance activities on the Offline Files cache. It is recommended to disable this service during the repackaging of a software.", false, serviceController.Status == ServiceControllerStatus.Running || serviceController.Status == ServiceControllerStatus.StartPending);
                            break;

                        case "wsearch":
                            yield return new ServiceRecommendation(serviceController.ServiceName, serviceController.DisplayName, "Indexing of files in the background can cause unnecessary noise. It is recommended to disable this service during the repackaging of a software. ", false, serviceController.Status == ServiceControllerStatus.Running || serviceController.Status == ServiceControllerStatus.StartPending);
                            break;
                            
                        case "wuauserv":
                            yield return new ServiceRecommendation(serviceController.ServiceName, serviceController.DisplayName, "Windows Update may periodically check and download a content which is not relevant for a repackaged app. It is recommended to disable this service during the repackaging of a software.", false, serviceController.Status == ServiceControllerStatus.Running || serviceController.Status == ServiceControllerStatus.StartPending);
                            break;

                        case "dps":
                            yield return new ServiceRecommendation(serviceController.ServiceName, serviceController.DisplayName, "Disabling this service may improve performance and quality of repackaged packages.", false, serviceController.Status == ServiceControllerStatus.Running || serviceController.Status == ServiceControllerStatus.StartPending);
                            break;

                        case "msmpengs":
                            yield return new ServiceRecommendation(serviceController.ServiceName, serviceController.DisplayName, "For the best repackaging experience, anti-virus software should be disabled when possible.", false, serviceController.Status == ServiceControllerStatus.Running || serviceController.Status == ServiceControllerStatus.StartPending);
                            break;
                    }
                }
            }
        }
    }
}