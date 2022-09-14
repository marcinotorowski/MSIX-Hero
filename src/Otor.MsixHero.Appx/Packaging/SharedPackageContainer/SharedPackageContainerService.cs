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
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Management.Automation;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using Otor.MsixHero.Appx.Packaging.SharedPackageContainer.Entities;
using Otor.MsixHero.Appx.Packaging.SharedPackageContainer.Exceptions;
using Otor.MsixHero.Infrastructure.Helpers;
using Otor.MsixHero.Infrastructure.ThirdParty.PowerShell;

namespace Otor.MsixHero.Appx.Packaging.SharedPackageContainer;

public class SharedPackageContainerService : ISharedPackageContainerService
{
    public async Task<IList<Entities.SharedPackageContainer>> GetAll(CancellationToken cancellationToken = default)
    {
        var list = new List<Entities.SharedPackageContainer>();

        using var powerShell = await PowerShellSession.CreateForAppxModule().ConfigureAwait(false);
        using var command = powerShell.AddCommand("Get-AppSharedPackageContainer");

        PSDataCollection<PSObject> results;
        try
        {
            results = await powerShell.InvokeAsync().ConfigureAwait(false);
        }
        catch (CommandNotFoundException e)
        {
            throw new NotSupportedException(Resources.Localization.Packages_Error_SharedContainerNotSupported, e);
        }

        foreach (var result in results)
        {

            list.Add(PsObjectToSharedContainer(result));
        }

        return list;
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

        string containerName;

        await using (var openStream = File.OpenRead(containerFile.FullName))
        {
            var file = (Entities.SharedPackageContainer)new XmlSerializer(typeof(Entities.SharedPackageContainer)).Deserialize(openStream);
            if (file == null)
            {
                throw new InvalidOperationException("Empty file.");
            }

            containerName = file.Name;
            var getAll = await this.GetRegisteredFamilyNames(cancellationToken).ConfigureAwait(false);

            var findFirst = file.PackageFamilies?.FirstOrDefault(pf => getAll.TryGetValue(pf.FamilyName, out var container) && container != file.Name);
            if (findFirst != null)
            {
                throw new AlreadyInAnotherContainerException(getAll[findFirst.FamilyName], findFirst.FamilyName);
            }
        }

        using (var powerShell = await PowerShellSession.CreateForAppxModule().ConfigureAwait(false))
        {
            using var command = powerShell.AddCommand("Add-AppSharedPackageContainer");

            command.AddParameter("Path", containerFile.FullName);

            switch (containerConflictResolution)
            {
                case ContainerConflictResolution.Merge:
                    command.AddParameter("Merge");
                    break;
                case ContainerConflictResolution.Replace:
                    command.AddParameter("Force");
                    break;
            }

            if (forceApplicationShutdown)
            {
                command.AddParameter("ForceApplicationShutdown");
            }

            try
            {
                await powerShell.InvokeAsync().ConfigureAwait(false);
            }
            catch (CommandNotFoundException e)
            {
                throw new NotSupportedException("", e);
            }
        }

        return await this.GetByName(containerName, cancellationToken).ConfigureAwait(false);
    }

    public async Task Remove(string containerName, bool forceApplicationShutdown = false, CancellationToken cancellationToken = default)
    {
        using var powerShell = await PowerShellSession.CreateForAppxModule().ConfigureAwait(false);
        using var command = powerShell.AddCommand("Remove-AppSharedPackageContainer");
        command.AddParameter("Name", containerName);

        if (forceApplicationShutdown)
        {
            command.AddParameter("ForceApplicationShutdown");
        }

        // Note: Command let has also a property -AllUsers, but it is not implemented and always throws.

        try
        {
            await powerShell.InvokeAsync().ConfigureAwait(false);
        }
        catch (CommandNotFoundException e)
        {
            throw new NotSupportedException(Resources.Localization.Packages_Error_SharedContainerNotSupported, e);
        }
    }

    public async Task<Entities.SharedPackageContainer> GetByName(string containerName, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(containerName))
        {
            throw new ArgumentNullException(nameof(containerName));
        }

        using var powerShell = await PowerShellSession.CreateForAppxModule().ConfigureAwait(false);
        using var command = powerShell.AddCommand("Get-AppSharedPackageContainer");

        command.AddParameter("Name", containerName);

        PSDataCollection<PSObject> results;
        try
        {
            results = await powerShell.InvokeAsync().ConfigureAwait(false);
        }
        catch (CommandNotFoundException e)
        {
            throw new NotSupportedException(Resources.Localization.Packages_Error_SharedContainerNotSupported, e);
        }

        var result = results.FirstOrDefault();
        if (result == null)
        {
            return null;
        }

        return PsObjectToSharedContainer(result);
    }

    public async Task Reset(string containerName, CancellationToken cancellationToken = default)
    {
        using var powerShell = await PowerShellSession.CreateForAppxModule().ConfigureAwait(false);
        using var command = powerShell.AddCommand("Reset-AppSharedPackageContainer");
        command.AddParameter("Name", containerName);
        command.AddParameter("Force");

        try
        {
            await powerShell.InvokeAsync().ConfigureAwait(false);
        }
        catch (CommandNotFoundException e)
        {
            throw new NotSupportedException(Resources.Localization.Packages_Error_SharedContainerNotSupported, e);
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

    private static Entities.SharedPackageContainer PsObjectToSharedContainer(PSObject result)
    {
        var name = (string)result.Properties["Name"]?.Value;
        var id = (string)result.Properties["Id"]?.Value;

        var obj = new Entities.SharedPackageContainer
        {
            Name = name,
            Id = id,
            PackageFamilies = new List<SharedPackageFamily>()
        };

        if (result.Properties["PackageFamilyNames"].Value.GetType().GetProperty("BaseObject", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public)?.GetValue(result.Properties["PackageFamilyNames"].Value) is ArrayList packageList)
        {
            foreach (var pkg in packageList.OfType<string>())
            {
                obj.PackageFamilies.Add(new SharedPackageFamily() { FamilyName = pkg });
            }
        }

        return obj;
    }
}