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
using Otor.MsixHero.Infrastructure.Helpers;

namespace Otor.MsixHero.Appx.Packaging.SharedPackageContainer;

public class AppxSharedPackageContainerWin10MockService : IAppxSharedPackageContainerService
{
    public async Task<Entities.SharedPackageContainer> Add(Entities.SharedPackageContainer container, bool forceApplicationShutdown = false,
        ContainerConflictResolution containerConflictResolution = ContainerConflictResolution.Default,
        CancellationToken cancellationToken = default)
    {
        var all = this.Read();

        var findExisting = all.FirstOrDefault(c => string.Equals(c.Name, container.Name, StringComparison.OrdinalIgnoreCase));
        
        var packageAlreadyPresent = all.Where(c => c != findExisting).SelectMany(c => c.PackageFamilies).Select(c => c.FamilyName).Intersect(container.PackageFamilies.Select(c => c.FamilyName), StringComparer.OrdinalIgnoreCase).FirstOrDefault();
        if (packageAlreadyPresent != null)
        {
            throw new Exception("Package " + packageAlreadyPresent + " is already in another container");
        }

        if (findExisting == null)
        {
            container.Id = Guid.NewGuid().ToString();
            all.Add(container);
        }
        else if (containerConflictResolution == ContainerConflictResolution.Replace)
        {
            all.Remove(findExisting);
            all.Add(container);

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

        await Task.Delay(TimeSpan.FromMilliseconds(500), cancellationToken).ConfigureAwait(false);
        this.Save(all);

        return container;
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

    public async Task<IList<Entities.SharedPackageContainer>> GetAll(CancellationToken cancellationToken = default)
    {
        await Task.Delay(1000, cancellationToken).ConfigureAwait(false);
        return this.Read();
    }

    public Task Remove(string containerName, bool forceApplicationShutdown = false, CancellationToken cancellationToken = default)
    {
        var all = this.Read();
        var find = all.FirstOrDefault(c => string.Equals(c.Name, containerName, StringComparison.OrdinalIgnoreCase));
        if (find != null)
        {
            all.Remove(find);
        }

        this.Save(all);
        return Task.CompletedTask;
    }

    public Task<Entities.SharedPackageContainer> GetByName(string containerName, CancellationToken cancellationToken = default)
    {
        var all = this.Read();
        return Task.FromResult(all.FirstOrDefault(c => string.Equals(c.Name, containerName, StringComparison.OrdinalIgnoreCase)));
    }

    public Task Reset(string containerName, CancellationToken cancellationToken = default)
    {
        return Task.Delay(TimeSpan.FromSeconds(3), cancellationToken);
    }

    public bool IsSharedPackageContainerSupported()
    {
        return true;
    }

    public bool IsAdminRequiredToManage()
    {
        return false;
    }

    private void Save(List<Entities.SharedPackageContainer> list)
    {
        var file = new FileInfo(Path.Combine(Path.GetTempPath(), "msix-hero-win10-spc.xml"));
        if (file.Exists)
        {
            file.Delete();
        }
        else if (file.Directory?.Exists == false)
        {
            file.Directory.Create();
        }

        var serializer = new XmlSerializer(typeof(List<Entities.SharedPackageContainer>));
        using var fs = File.OpenWrite(file.FullName);
        serializer.Serialize(fs, list);
    }

    private List<Entities.SharedPackageContainer> Read()
    {
        var file = new FileInfo(Path.Combine(Path.GetTempPath(), "msix-hero-win10-spc.xml"));
        if (!file.Exists)
        {
            return new List<Entities.SharedPackageContainer>();
        }

        var serializer = new XmlSerializer(typeof(List<Entities.SharedPackageContainer>));
        using var fs = File.OpenRead(file.FullName);
        // ReSharper disable once AccessToDisposedClosure
        return ExceptionGuard.Guard(() => (List<Entities.SharedPackageContainer>)serializer.Deserialize(fs)) ?? new List<Entities.SharedPackageContainer>();
    }
}