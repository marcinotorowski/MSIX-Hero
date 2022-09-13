using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Otor.MsixHero.Appx.Packaging.SharedPackageContainer
{
    public interface ISharedPackageContainerService
    {
        Task<Entities.SharedPackageContainer> Add(
            Entities.SharedPackageContainer container, 
            bool forceApplicationShutdown = false,
            ContainerConflictResolution containerConflictResolution = ContainerConflictResolution.Default,
            CancellationToken cancellationToken = default);
        
        Task<Entities.SharedPackageContainer> Add(
            FileInfo containerFile,
            bool forceApplicationShutdown = false,
            ContainerConflictResolution containerConflictResolution = ContainerConflictResolution.Default,
            CancellationToken cancellationToken = default);

        Task<IList<Entities.SharedPackageContainer>> GetAll(CancellationToken cancellationToken = default);

        Task Remove(
            string containerName,
            bool forceApplicationShutdown = false,
            CancellationToken cancellationToken = default);

        Task<Entities.SharedPackageContainer> GetByName(
            string containerName,
            CancellationToken cancellationToken = default);
        
        Task Reset(
            string containerName, 
            CancellationToken cancellationToken = default);

        bool IsSharedPackageContainerSupported();
    }
}
