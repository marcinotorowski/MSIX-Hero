using System;
using System.Threading;
using System.Threading.Tasks;
using Otor.MsixHero.Appx.Diagnostic.Logging;
using Otor.MsixHero.Appx.Diagnostic.Registry;
using Otor.MsixHero.Appx.Packaging.Installation;
using Otor.MsixHero.Appx.Signing;
using Otor.MsixHero.Appx.Volumes;
using Otor.MsixHero.Appx.WindowsVirtualDesktop.AppAttach;
using Otor.MsixHero.Infrastructure.Helpers;
using Otor.MsixHero.Infrastructure.Processes.Ipc;
using Otor.MsixHero.Infrastructure.Processes.SelfElevation;
using Otor.MsixHero.Infrastructure.Processes.SelfElevation.Enums;
using Otor.MsixHero.Infrastructure.Services;
using Otor.MsixHero.Lib.Proxy.Diagnostic;
using Otor.MsixHero.Lib.Proxy.Packaging;
using Otor.MsixHero.Lib.Proxy.Signing;
using Otor.MsixHero.Lib.Proxy.Volumes;
using Otor.MsixHero.Lib.Proxy.WindowsVirtualDesktop;

namespace Otor.MsixHero.Lib.Proxy
{
    public class SelfElevationManagerFactory : 
        ISelfElevationProxyProvider<ISigningManager>,
        ISelfElevationProxyProvider<IAppxVolumeManager>,
        ISelfElevationProxyProvider<IAppxPackageManager>,
        ISelfElevationProxyProvider<IAppAttachManager>,
        ISelfElevationProxyProvider<IRegistryManager>,
        ISelfElevationProxyProvider<IAppxLogManager>
    {
        private readonly IElevatedClient client;
        private readonly IConfigurationService configurationService;

        public SelfElevationManagerFactory(IElevatedClient client, IConfigurationService configurationService)
        {
            this.client = client;
            this.configurationService = configurationService;
        }
        
        async Task<ISigningManager> ISelfElevationProxyProvider<ISigningManager>.GetProxyFor(SelfElevationLevel selfElevationLevel, CancellationToken cancellationToken)
        {
            if (selfElevationLevel == SelfElevationLevel.HighestAvailable)
            {
                selfElevationLevel = await this.GetMaxAvailableElevation(cancellationToken).ConfigureAwait(false);
            }

            switch (selfElevationLevel)
            {
                case SelfElevationLevel.AsInvoker:
                    return new SigningManager();
                case SelfElevationLevel.AsAdministrator:
                    if (await UserHelper.IsAdministratorAsync(cancellationToken).ConfigureAwait(false))
                    {
                        return new SigningManager();
                    }

                    return new SigningManagerElevationProxy(this.client, this);
                default:
                    throw new InvalidOperationException("Elevation API returned wrong results.");
            }
        }
        
        async Task<IAppAttachManager> ISelfElevationProxyProvider<IAppAttachManager>.GetProxyFor(SelfElevationLevel selfElevationLevel, CancellationToken cancellationToken)
        {
            if (selfElevationLevel == SelfElevationLevel.HighestAvailable)
            {
                selfElevationLevel = await this.GetMaxAvailableElevation(cancellationToken).ConfigureAwait(false);
            }

            switch (selfElevationLevel)
            {
                case SelfElevationLevel.AsInvoker:
                    return new AppAttachManager(this); // return user
                case SelfElevationLevel.AsAdministrator:
                    if (await UserHelper.IsAdministratorAsync(cancellationToken).ConfigureAwait(false))
                    {
                        return new AppAttachManager(this);
                    }

                    return new AppAttachManagerElevationProxy(this.client); // return admin;
                default:
                    throw new InvalidOperationException("Elevation API returned wrong results.");
            }
        }

        async Task<IAppxVolumeManager> ISelfElevationProxyProvider<IAppxVolumeManager>.GetProxyFor(SelfElevationLevel selfElevationLevel, CancellationToken cancellationToken)
        {
            if (selfElevationLevel == SelfElevationLevel.HighestAvailable)
            {
                selfElevationLevel = await this.GetMaxAvailableElevation(cancellationToken).ConfigureAwait(false);
            }

            switch (selfElevationLevel)
            {
                case SelfElevationLevel.AsInvoker:
                    return new AppxVolumeManager();
                case SelfElevationLevel.AsAdministrator:
                    if (await UserHelper.IsAdministratorAsync(cancellationToken).ConfigureAwait(false))
                    {
                        return new AppxVolumeManager();
                    }

                    return new AppxVolumeManagerElevationProxy(this.client, this);
                default:
                    throw new InvalidOperationException("Elevation API returned wrong results.");
            }
        }

        async Task<IRegistryManager> ISelfElevationProxyProvider<IRegistryManager>.GetProxyFor(SelfElevationLevel selfElevationLevel, CancellationToken cancellationToken)
        {
            if (selfElevationLevel == SelfElevationLevel.HighestAvailable)
            {
                selfElevationLevel = await this.GetMaxAvailableElevation(cancellationToken).ConfigureAwait(false);
            }

            switch (selfElevationLevel)
            {
                case SelfElevationLevel.AsInvoker:
                    return new RegistryManager();
                case SelfElevationLevel.AsAdministrator:
                    if (await UserHelper.IsAdministratorAsync(cancellationToken).ConfigureAwait(false))
                    {
                        return new RegistryManager();
                    }

                    return new RegistryManagerElevationProxy(this.client);
                default:
                    throw new InvalidOperationException("Elevation API returned wrong results.");
            }
        }

        async Task<IAppxLogManager> ISelfElevationProxyProvider<IAppxLogManager>.GetProxyFor(SelfElevationLevel selfElevationLevel, CancellationToken cancellationToken)
        {
            if (selfElevationLevel == SelfElevationLevel.HighestAvailable)
            {
                selfElevationLevel = await this.GetMaxAvailableElevation(cancellationToken).ConfigureAwait(false);
            }

            switch (selfElevationLevel)
            {
                case SelfElevationLevel.AsInvoker:
                    return new AppxLogManager();
                case SelfElevationLevel.AsAdministrator:
                    if (await UserHelper.IsAdministratorAsync(cancellationToken).ConfigureAwait(false))
                    {
                        return new AppxLogManager();
                    }

                    return new AppxLogManagerElevationProxy(this.client);
                default:
                    throw new InvalidOperationException("Elevation API returned wrong results.");
            }
        }

        async Task<IAppxPackageManager> ISelfElevationProxyProvider<IAppxPackageManager>.GetProxyFor(SelfElevationLevel selfElevationLevel, CancellationToken cancellationToken)
        {
            if (selfElevationLevel == SelfElevationLevel.HighestAvailable)
            {
                selfElevationLevel = await this.GetMaxAvailableElevation(cancellationToken).ConfigureAwait(false);
            }

            switch (selfElevationLevel)
            {
                case SelfElevationLevel.AsInvoker:
                    return new AppxPackageManager(new RegistryManager(), this.configurationService);
                case SelfElevationLevel.AsAdministrator:
                    if (await UserHelper.IsAdministratorAsync(cancellationToken).ConfigureAwait(false))
                    {
                        return new AppxPackageManager(new RegistryManager(), this.configurationService);
                    }

                    return new AppxPackageManagerElevationProxy(this.client);
                default:
                    throw new InvalidOperationException("Elevation API returned wrong results.");
            }
        }

        private async Task<SelfElevationLevel> GetMaxAvailableElevation(CancellationToken cancellationToken)
        {
            if (await UserHelper.IsAdministratorAsync(cancellationToken).ConfigureAwait(false))
            {
                return SelfElevationLevel.AsAdministrator;
            }

            var isProcessRunning = await this.client.Test(cancellationToken).ConfigureAwait(false);
            return isProcessRunning ? SelfElevationLevel.AsAdministrator : SelfElevationLevel.AsInvoker;
        }
    }
}