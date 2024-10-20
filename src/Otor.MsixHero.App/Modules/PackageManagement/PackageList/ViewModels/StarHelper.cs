using Otor.MsixHero.Appx.Packaging.Installation.Entities;
using Otor.MsixHero.Appx.Packaging.Interop;
using Otor.MsixHero.Appx.Packaging;
using System;
using System.Collections.Generic;
using System.Linq;
using Otor.MsixHero.Appx.Packaging.Manifest.Enums;
using Otor.MsixHero.Infrastructure.Configuration;
using Otor.MsixHero.Appx.Common.Identity;

namespace Otor.MsixHero.App.Modules.PackageManagement.PackageList.ViewModels
{
    internal class PackageStarHelper
    {
        private static readonly Version WildcardVersion = new Version(0, 0, 0, 0);

        private readonly Configuration _configuration;
        private readonly Dictionary<string, string> _cachedPublisherHashes = new();
        private Dictionary<string, IList<PackageIdentity>> _starred;

        public PackageStarHelper(Configuration configuration)
        {
            _configuration = configuration;
        }

        public bool IsStarred(string publisher, string name, Version version, AppxPackageArchitecture architecture, string resourceId)
        {
            if (this._starred == null)
            {
                this._starred = new Dictionary<string, IList<PackageIdentity>>();

                foreach (var item in this._configuration?.Packages?.StarredApps ?? Enumerable.Empty<string>())
                {
                    if (!PackageIdentity.TryFromFullName(item, out var identity))
                    {
                        continue;
                    }

                    if (!this._starred.TryGetValue(identity.AppName, out var appIdentities))
                    {
                        appIdentities = new List<PackageIdentity>();
                        this._starred[identity.AppName] = appIdentities;
                    }

                    appIdentities.Add(identity);
                }
            }

            var isStarred = false;
            if (this._starred.TryGetValue(name, out var similarIdentities))
            {
                if (similarIdentities.Any(appIdentity =>
                {
                    if ((appIdentity.AppVersion == WildcardVersion || version == appIdentity.AppVersion) &&
                        resourceId == appIdentity.ResourceId &&
                        architecture == appIdentity.Architecture)
                    {
                        // do the publisher comparison only at the very end, in most cases one of the previous conditions will make it
                        // obsolete, and we do not have to calculate familyName and publisherHash.
                        if (!this._cachedPublisherHashes.TryGetValue(publisher, out var publisherHashId))
                        {
                            publisherHashId = AppxPackagingNameHelper.GetPublisherHash(publisher);
                            this._cachedPublisherHashes[publisher] = publisherHashId;
                        }

                        return publisherHashId == appIdentity.PublisherHash;
                    }

                    return false;
                }))
                {
                    isStarred = true;
                }
            }

            return isStarred;
        }
    }
}
