using System.Diagnostics.CodeAnalysis;
using System.IO;
using Otor.MsixHero.Appx.Packaging;
using Otor.MsixHero.Appx.Reader.Manifest.Entities;

namespace Otor.MsixHero.App.Hero.Commands.Packages;

[SuppressMessage("ReSharper", "IdentifierTypo")]
// ReSharper disable once InconsistentNaming
public record struct PackageLUID
{
    public PackageLUID()
    {
        this.Id = CreateId(null, null);
    }

    public PackageLUID(FileInfo fileInfo)
    {
        this.Id = CreateId(fileInfo.FullName, null);
    }

    public PackageLUID(string fullName)
    {
        this.Id = CreateId(null, fullName);
    }

    public PackageLUID(string filePath, string fullName)
    {
        this.Id = CreateId(filePath, fullName);
    }

    public PackageLUID(PackageEntry packageEntry)
    {
        this.Id = CreateId(packageEntry.ManifestPath, packageEntry.PackageFullName);
    }

    public PackageLUID(AppxPackage packageEntry)
    {
        this.Id = CreateId(packageEntry.PackagePath, packageEntry.FullName);
    }

    public string Id { get; }

    private static string CreateId(string path, string fullName)
    {
        return path + ";" + fullName;
    }
}