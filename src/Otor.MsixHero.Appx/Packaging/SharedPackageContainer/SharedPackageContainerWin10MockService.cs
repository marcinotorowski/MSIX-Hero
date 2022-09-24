// MSIX Hero
// Copyright (C) 2022 Marcin Otorowski
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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Otor.MsixHero.Appx.Packaging.SharedPackageContainer.Entities;

namespace Otor.MsixHero.Appx.Packaging.SharedPackageContainer;

public class SharedPackageContainerWin10MockService : ISharedPackageContainerService
{
    private static IList<Entities.SharedPackageContainer> _all = new List<Entities.SharedPackageContainer>()
    {
        new()
        {
            Id = Guid.NewGuid().ToString(),
            Name = "MSIX Hero Dummy Container",
            PackageFamilies = new List<SharedPackageFamily>
            {
                new SharedPackageFamily
                {
                    FamilyName = "MSIXHero_zxq1da1qqbeze"
                }
            }
        }
    };

    public Task<Entities.SharedPackageContainer> Add(Entities.SharedPackageContainer container, bool forceApplicationShutdown = false,
        ContainerConflictResolution containerConflictResolution = ContainerConflictResolution.Default,
        CancellationToken cancellationToken = default)
    {
        var findExisting = _all.FirstOrDefault(c => string.Equals(c.Name, container.Name, StringComparison.OrdinalIgnoreCase));
        
        var packageAlreadyPresent = _all.Where(c => c != findExisting).SelectMany(c => c.PackageFamilies).Select(c => c.FamilyName).Intersect(container.PackageFamilies.Select(c => c.FamilyName), StringComparer.OrdinalIgnoreCase).FirstOrDefault();
        if (packageAlreadyPresent != null)
        {
            return Task.FromException<Entities.SharedPackageContainer>(new Exception("Package " + packageAlreadyPresent + " is already in another container"));
        }

        if (findExisting == null)
        {
            container.Id = Guid.NewGuid().ToString();
            _all.Add(container);
        }
        else if (containerConflictResolution == ContainerConflictResolution.Replace)
        {
            _all.Remove(findExisting);
            _all.Add(container);

            container.Id = Guid.NewGuid().ToString();
        }
        else
        {
            var newPackages = container.PackageFamilies.Select(p => p.FamilyName).Except(findExisting.PackageFamilies.Select(f => f.FamilyName), StringComparer.OrdinalIgnoreCase);
            foreach (var newPackage in newPackages)
            {
                findExisting.PackageFamilies.Add(new SharedPackageFamily() { FamilyName = newPackage });
            }

            container = findExisting;
        }

        return Task.FromResult(container);
    }

    public async Task<Entities.SharedPackageContainer> Add(FileInfo containerFile, bool forceApplicationShutdown = false,
        ContainerConflictResolution containerConflictResolution = ContainerConflictResolution.Default,
        CancellationToken cancellationToken = default)
    {
        if (!containerFile.Exists)
        {
            throw new FileNotFoundException();
        }

        var serializer = new XmlSerializer(typeof(Entities.SharedPackageContainer));
        
        await using var stream = File.OpenRead(containerFile.FullName);
        return await this.Add((Entities.SharedPackageContainer)serializer.Deserialize(stream), forceApplicationShutdown, containerConflictResolution, cancellationToken).ConfigureAwait(false);
    }

    public Task<IList<Entities.SharedPackageContainer>> GetAll(CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_all);
    }

    public Task Remove(string containerName, bool forceApplicationShutdown = false, CancellationToken cancellationToken = default)
    {
        var find = _all.FirstOrDefault(c => string.Equals(c.Name, containerName, StringComparison.OrdinalIgnoreCase));
        if (find != null)
        {
            _all.Remove(find);
        }

        return Task.CompletedTask;
    }

    public Task<Entities.SharedPackageContainer> GetByName(string containerName, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_all.FirstOrDefault(c => string.Equals(c.Name, containerName, StringComparison.OrdinalIgnoreCase)));
    }

    public Task Reset(string containerName, CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }

    public bool IsSharedPackageContainerSupported()
    {
        return true;
    }
}