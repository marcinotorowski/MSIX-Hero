using System;

namespace Otor.MsixHero.Appx.Packaging.Services;

[Serializable]
public struct PackageQuerySource
{
    public PackageQuerySourceType Type { get; set; }

    public string Path { get; set; }

    public static PackageQuerySource InstalledForCurrentUser() => new() { Type = PackageQuerySourceType.InstalledForCurrentUser };
    
    public static PackageQuerySource InstalledForAllUsers() => new() { Type = PackageQuerySourceType.InstalledForAllUsers };
    
    public static PackageQuerySource Installed() => new() { Type = PackageQuerySourceType.Installed };

    public static PackageQuerySource FromFolder(string folderPath, bool recurse = false) => new()
    {
        Type = PackageQuerySourceType.Directory, Path = recurse ? System.IO.Path.Combine(folderPath, "*") : folderPath
    };
}