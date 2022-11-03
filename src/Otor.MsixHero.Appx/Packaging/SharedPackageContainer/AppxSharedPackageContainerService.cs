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
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using Windows.Management.Deployment;
using Otor.MsixHero.Appx.Packaging.SharedPackageContainer.Entities;
using Otor.MsixHero.Appx.Packaging.SharedPackageContainer.Exceptions;
using Otor.MsixHero.Infrastructure.Helpers;

namespace Otor.MsixHero.Appx.Packaging.SharedPackageContainer;

public class AppxSharedPackageContainerService : IAppxSharedPackageContainerService
{
    private Lazy<SharedPackageContainerManager> _manager = new(SharedPackageContainerManager.GetDefault);

    public Task<IList<Entities.SharedPackageContainer>> GetAll(CancellationToken cancellationToken = default)
    {
        var containers = this._manager.Value.FindContainers(new FindSharedPackageContainerOptions());

        IList<Entities.SharedPackageContainer> list = new List<Entities.SharedPackageContainer>();
        foreach (var result in containers)
        {

            list.Add(SourceToSharedContainer(result));
        }

        return Task.FromResult(list);
    }

    public async Task<Entities.SharedPackageContainer> Add(
        Entities.SharedPackageContainer container,
        bool forceApplicationShutdown = false,
        ContainerConflictResolution containerConflictResolution = ContainerConflictResolution.Default,
        CancellationToken cancellationToken = default)
    {
        var temporaryFile = new FileInfo(Path.Combine(Path.GetTempPath(), "shared-app-container-" + Guid.NewGuid().ToString("N").Substring(0, 8) + ".xml"));

        if (temporaryFile.Exists)
        {
            temporaryFile.Delete();
        }
        else if (temporaryFile.Directory?.Exists == false)
        {
            temporaryFile.Directory.Create();
        }

        try
        {
            var serializer = new XmlSerializer(typeof(Entities.SharedPackageContainer));

            await using (var fileStream = File.OpenWrite(temporaryFile.FullName))
            {
                await using var fileStreamWriter = new StreamWriter(fileStream, Encoding.UTF8);
                var settings = new XmlWriterSettings
                {
                    Encoding = Encoding.UTF8,
                    Indent = true,
                    NewLineOnAttributes = false,
                    OmitXmlDeclaration = false,
                    Async = true
                };

                await using var xmlBodyWriter = XmlWriter.Create(fileStreamWriter, settings);

                var ns = new XmlSerializerNamespaces();
                ns.Add(string.Empty, string.Empty);
                serializer.Serialize(xmlBodyWriter, container, ns);

                await xmlBodyWriter.FlushAsync().ConfigureAwait(false);
                await fileStream.FlushAsync(cancellationToken).ConfigureAwait(false);
            }

            temporaryFile.Refresh();
            return await this.Add(temporaryFile, forceApplicationShutdown, containerConflictResolution, cancellationToken).ConfigureAwait(false);
        }
        finally
        {
            ExceptionGuard.Guard(() => temporaryFile.Delete());
        }
    }

    public async Task<Entities.SharedPackageContainer> Add(
        FileInfo containerFile,
        bool forceApplicationShutdown = false,
        ContainerConflictResolution containerConflictResolution = ContainerConflictResolution.Default,
        CancellationToken cancellationToken = default)
    {
        if (!containerFile.Exists)
        {
            throw new FileNotFoundException(Resources.Localization.Packages_Error_FileNotFound, containerFile.FullName);
        }

        await using var openStream = File.OpenRead(containerFile.FullName);
        var file = (Entities.SharedPackageContainer)new XmlSerializer(typeof(Entities.SharedPackageContainer)).Deserialize(openStream);
        if (file == null)
        {
            throw new InvalidOperationException("Empty file.");
        }

        var getAll = await this.GetRegisteredFamilyNames(cancellationToken).ConfigureAwait(false);

        var findFirst = file.PackageFamilies?.FirstOrDefault(pf => getAll.TryGetValue(pf.FamilyName, out var container) && container != file.Name);
        if (findFirst != null)
        {
            throw new AlreadyInAnotherContainerException(getAll[findFirst.FamilyName], findFirst.FamilyName);
        }

        var options = new CreateSharedPackageContainerOptions
        {
            ForceAppShutdown = forceApplicationShutdown
        };

        switch (containerConflictResolution)
        {
            case ContainerConflictResolution.Merge:
                options.CreateCollisionOption = SharedPackageContainerCreationCollisionOptions.MergeWithExisting;
                break;
            case ContainerConflictResolution.Replace:
                options.CreateCollisionOption = SharedPackageContainerCreationCollisionOptions.ReplaceExisting;
                break;
            default:
                options.CreateCollisionOption = SharedPackageContainerCreationCollisionOptions.FailIfExists;
                break;
        }

        foreach (var item in file.PackageFamilies?.Select(x => x.FamilyName) ?? Enumerable.Empty<string>())
        {
            options.Members.Add(new SharedPackageContainerMember(item));
        }

        var result = this._manager.Value.CreateContainer(file.Name, options);
        switch (result.Status)
        {
            case SharedPackageContainerOperationStatus.Success:
                return await this.GetByName(file.Name, cancellationToken).ConfigureAwait(false);

            case SharedPackageContainerOperationStatus.BlockedByPolicy:
                throw result.ExtendedError; // todo: wrap in some exception?

            case SharedPackageContainerOperationStatus.AlreadyExists:
                throw result.ExtendedError; // todo: wrap in some exception?

            case SharedPackageContainerOperationStatus.PackageFamilyExistsInAnotherContainer:
                throw result.ExtendedError; // todo: wrap in some exception?
                    
            default:
                throw result.ExtendedError;
        }
    }

    public async Task Remove(string containerName, bool forceApplicationShutdown = false, CancellationToken cancellationToken = default)
    {
        var container = await this.GetByName(containerName, cancellationToken).ConfigureAwait(false);
        if (container == null)
        {
            return;
        }

        var options = new DeleteSharedPackageContainerOptions
        {
            ForceAppShutdown = forceApplicationShutdown
        };

        var result = this._manager.Value.DeleteContainer(container.Id, options);
        
        switch (result.Status)
        {
            case SharedPackageContainerOperationStatus.Success:
            case SharedPackageContainerOperationStatus.NotFound:
                return;
            default:
                throw result.ExtendedError;
        }
    }

    public Task<Entities.SharedPackageContainer> GetByName(string containerName, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(containerName))
        {
            throw new ArgumentNullException(nameof(containerName));
        }

        var container = this._manager.Value.FindContainers(new FindSharedPackageContainerOptions
        {
            Name = containerName
        }).FirstOrDefault();
        
        return Task.FromResult(SourceToSharedContainer(container));
    }

    public Task Reset(string containerName, CancellationToken cancellationToken = default)
    {
        var container = this._manager.Value.FindContainers(new FindSharedPackageContainerOptions
        {
            Name = containerName
        }).FirstOrDefault();

        if (container == null)
        {
            throw new InvalidOperationException();
        }

        var result = container.ResetData();
        switch (result.Status)
        {
            case SharedPackageContainerOperationStatus.Success:
                return Task.CompletedTask;
            default:
                return Task.FromException(result.ExtendedError);
        }
    }

    /// <remarks>
    /// Shared package containers allows IT Pros to create a shared runtime container for MSIX packaged
    /// application – sharing a merged view of the virtual file system and virtual registry - enabling access
    /// to one another’s package root files and state. Beginning on Windows 10 Insider Preview Build 21354,
    /// IT Pros will be able to manage what apps can be in what container is important to the conversion of
    /// MSIX from legacy installers.
    /// https://docs.microsoft.com/en-us/windows/msix/manage/shared-package-container
    /// </remarks>
    public bool IsSharedPackageContainerSupported()
    {
        var minimumSupportedVersion = new Version(10, 0, 21354, 0);
        var currentVersion = NdDll.RtlGetVersion();

        return currentVersion >= minimumSupportedVersion;
    }

    public bool IsAdminRequiredToManage()
    {
        return true;
    }

    private async Task<IDictionary<string, string>> GetRegisteredFamilyNames(CancellationToken cancellationToken)
    {
        var all = await this.GetAll(cancellationToken).ConfigureAwait(false);

        var familyNameToContainerMapping = new Dictionary<string, string>();

        foreach (var item in all.Where(c => c.PackageFamilies != null))
        {
            foreach (var pf in item.PackageFamilies.Where(c => c.FamilyName != null))
            {
                familyNameToContainerMapping[pf.FamilyName] = item.Name;
            }
        }

        return familyNameToContainerMapping;
    }

    private static Entities.SharedPackageContainer SourceToSharedContainer(Windows.Management.Deployment.SharedPackageContainer result)
    {
        if (result == null)
        {
            return null;
        }

        var name = result.Name;
        var id = result.Id;

        var obj = new Entities.SharedPackageContainer
        {
            Name = name,
            Id = id,
            PackageFamilies = new List<SharedPackageFamily>()
        };

        foreach (var pkg in result.GetMembers().Select(m => m.PackageFamilyName))
        {
            obj.PackageFamilies.Add(new SharedPackageFamily { FamilyName = pkg });
        }

        return obj;
    }
}