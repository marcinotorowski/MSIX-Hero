using System;
using System.Threading;
using System.Threading.Tasks;
using otor.msixhero.lib.BusinessLayer.Managers.AppAttach;
using otor.msixhero.lib.BusinessLayer.Managers.Packages;
using otor.msixhero.lib.BusinessLayer.Managers.Registry;
using otor.msixhero.lib.BusinessLayer.Managers.Signing;
using otor.msixhero.lib.BusinessLayer.Managers.Volumes;
using otor.msixhero.lib.Infrastructure.Configuration;
using otor.msixhero.lib.Infrastructure.Helpers;
using otor.msixhero.lib.Infrastructure.Ipc;

namespace otor.msixhero.lib.Infrastructure.SelfElevation
{
    public class SelfElevationManagerFactory : 
        ISelfElevationManagerFactory<ISigningManager>,
        ISelfElevationManagerFactory<IAppxVolumeManager>,
        ISelfElevationManagerFactory<IAppxPackageManager>,
        ISelfElevationManagerFactory<IAppAttachManager>,
        ISelfElevationManagerFactory<IRegistryManager>
    {
        private readonly IElevatedClient client;
        private readonly IConfigurationService configurationService;

        public SelfElevationManagerFactory(IElevatedClient client, IConfigurationService configurationService)
        {
            this.client = client;
            this.configurationService = configurationService;
        }
        
        async Task<ISigningManager> ISelfElevationManagerFactory<ISigningManager>.Get(SelfElevationLevel selfElevationLevel, CancellationToken cancellationToken)
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

                    return new ElevationProxySigningManager(this.client, this);
                default:
                    throw new InvalidOperationException("Elevation API returned wrong results.");
            }
        }
        
        async Task<IAppAttachManager> ISelfElevationManagerFactory<IAppAttachManager>.Get(SelfElevationLevel selfElevationLevel, CancellationToken cancellationToken)
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

                    return new ElevationProxyAppAttachManager(this.client); // return admin;
                default:
                    throw new InvalidOperationException("Elevation API returned wrong results.");
            }
        }

        async Task<IAppxVolumeManager> ISelfElevationManagerFactory<IAppxVolumeManager>.Get(SelfElevationLevel selfElevationLevel, CancellationToken cancellationToken)
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

                    return new ElevationProxyAppxVolumeManager(this.client, this);
                default:
                    throw new InvalidOperationException("Elevation API returned wrong results.");
            }
        }

        async Task<IRegistryManager> ISelfElevationManagerFactory<IRegistryManager>.Get(SelfElevationLevel selfElevationLevel, CancellationToken cancellationToken)
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

                    return new ElevationProxyRegistryManager(this.client);
                default:
                    throw new InvalidOperationException("Elevation API returned wrong results.");
            }
        }

        async Task<IAppxPackageManager> ISelfElevationManagerFactory<IAppxPackageManager>.Get(SelfElevationLevel selfElevationLevel, CancellationToken cancellationToken)
        {
            if (selfElevationLevel == SelfElevationLevel.HighestAvailable)
            {
                selfElevationLevel = await this.GetMaxAvailableElevation(cancellationToken).ConfigureAwait(false);
            }

            switch (selfElevationLevel)
            {
                case SelfElevationLevel.AsInvoker:
                    return new AppxPackageManager(this, this.configurationService);
                case SelfElevationLevel.AsAdministrator:
                    if (await UserHelper.IsAdministratorAsync(cancellationToken).ConfigureAwait(false))
                    {
                        return new AppxPackageManager(this, this.configurationService);
                    }

                    return new ElevationProxyAppxPackageManager(this.client);
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