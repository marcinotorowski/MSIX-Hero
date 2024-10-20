using Dapplo.Log;
using Otor.MsixHero.Appx.Common.Enums;
using Otor.MsixHero.Appx.Packaging.Installation;
using Otor.MsixHero.Appx.Reader.File.Adapters;
using Otor.MsixHero.Appx.Reader.File;
using Otor.MsixHero.Appx.Reader.Manifest.Entities;
using Otor.MsixHero.Appx.Reader.Manifest;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Otor.MsixHero.Appx.Packaging.Services
{
    public class AppxManifestReaderWithDependencyResolving(IAppxManifestReader actualReader) : IAppxManifestReader
    {
        private static readonly LogSource Logger = new();


        public async Task<AppxPackage> Read(IAppxFileReader fileReader, CancellationToken cancellationToken = default)
        {
            var result = await actualReader.Read(fileReader, cancellationToken).ConfigureAwait(false);


            foreach (var item in result.PackageDependencies)
            {
                cancellationToken.ThrowIfCancellationRequested();

                using IAppxFileReader tempReader = new PackageIdentityFileReaderAdapter(PackageManagerSingleton.Instance, PackageInstallationContext.CurrentUser, item.Name, item.Publisher, item.Version);
                try
                {
                    item.Dependency = await this.Read(tempReader, cancellationToken).ConfigureAwait(false);
                }
                catch (Exception)
                {
                    Logger.Debug().WriteLine("Could not read a dependency to {0} {2} by {1}", item.Name, item.Publisher, item.Version);
                }
            }

            return result;
        }

        public Task<AppxBundle> ReadBundle(IAppxFileReader fileReader, CancellationToken cancellationToken = default)
        {
            return actualReader.ReadBundle(fileReader, cancellationToken);
        }
    }
}
