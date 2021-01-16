#tool "nuget:?package=nuget.commandline&version=5.3.0"
var target = Argument("target", "Build");
var configuration = Argument("configuration", "PublishCore");

//////////////////////////////////////////////////////////////////////
// TASKS
//////////////////////////////////////////////////////////////////////

var outFolder = Argument("outFolder", "");
if (string.IsNullOrEmpty(outFolder))
{
    outFolder = System.IO.Path.Combine("out");
}

var binFolder = System.IO.Path.Combine(outFolder, "bin");
var msixFolder = System.IO.Path.Combine(outFolder, "msix");

var shouldSign = HasArgument("Sign")
    ? Argument<bool>("Sign")
    : false;

Task("Clean output folder")
    .Does(() =>
{
    Information("Cleaning folder '" + outFolder + "'...");
    CleanDirectory(outFolder);
});

Task("Build")
    .IsDependentOn("Clean output folder")
    .IsDependentOn("Publish .NET Core")
    .IsDependentOn("Trim publish folder")
    .IsDependentOn("Copy artifacts")
    .IsDependentOn("Sign MSIX");

string version = null;

Task("Determine version").Does(() =>{
    Information("Running git.exe to determine the version...");
    var p = StartProcess(
         "git",
         new ProcessSettings {
             Arguments = "describe --long",
             RedirectStandardOutput = true
         },
         out var redirectedStandardOutput,
         out var redirectedError
     );

    var v = string.Join("\r\n", redirectedStandardOutput);

    var r = System.Text.RegularExpressions.Regex.Match(v, @"v(\d+\.\d+)\-(\d+)\-g([a-z0-9]+)");
    if (r.Success)
    {
        version = r.Groups[1].Value + "." +r.Groups[2].Value + ".0";
    }
    else
    {
        r = System.Text.RegularExpressions.Regex.Match(v, @"v(\d+\.\d+)\.(\d+)(?:\.\d+)*\-(\d+)\-g([a-z0-9]+)");
        if (r.Success)
        {
            var cnt = int.Parse(r.Groups[2].Value) +int.Parse(r.Groups[3].Value);
            version = r.Groups[1].Value + "." + cnt + ".0";
        }
    }

    if (version == null)
    {
        throw new Exception("Unexpected git version " + v);
    }

    Information("Product version is '" + version + "'");

    if (BuildSystem.AppVeyor.IsRunningOnAppVeyor)
    {
        BuildSystem.AppVeyor.UpdateBuildVersion(version);
    }
});

Task("Publish .NET Framework")
.Does(() =>
{
    Information("Building Otor.MsixHero.DeviceGuardLoginHelper.csproj...");
    NuGetRestore("./src/Otor.MsixHero.DeviceGuardLoginHelper/Otor.MsixHero.DeviceGuardLoginHelper.csproj", new NuGetRestoreSettings { NoCache = true });
    MSBuild("./src/Otor.MsixHero.DeviceGuardLoginHelper/Otor.MsixHero.DeviceGuardLoginHelper.csproj", new MSBuildSettings {
        Verbosity = Verbosity.Minimal,
        ToolVersion = MSBuildToolVersion.VS2019,
        Configuration = "Release"
        });

    var src = System.IO.Path.Combine("src", "bin", "netcoreapp3.1", "DGSS");
    var tgt = System.IO.Path.Combine(binFolder, "DGSS");
    Information("Copying '" + src + "' to '" + tgt + "'...");
    CopyDirectory(src, tgt);
});

Task("Publish .NET Core")
    .IsDependentOn("Clean output folder")
    .IsDependentOn("Determine version")
    .IsDependentOn("Copy artifacts")
    .Does(() =>
{
    Information("Publishing 'Otor.MsixHero.sln'...");
    var settings = new DotNetCorePublishSettings
    {
        Configuration = configuration,
        OutputDirectory = binFolder
    };

    settings.MSBuildSettings = new DotNetCoreMSBuildSettings()
    .WithProperty("AssemblyVersion", version)
    .WithProperty("Version", version);
    DotNetCorePublish("./Otor.MsixHero.sln", settings);
});

Task("Trim publish folder")
    .IsDependentOn("Publish .NET Framework")
    .IsDependentOn("Publish .NET Core")
    .Does(() => {

        Information("Deleting all PDB files from '" + binFolder + "'...");
        DeleteFiles(System.IO.Path.Combine(binFolder, "*.pdb"));
        
        var removeFolder = new string[] {
            "cs",
            "de",
            "es",
            "fr",
            "it",
            "ja",
            "ko",
            "pl",
            "pt-br",
            "ru",
            "tr",
            "zh-Hans",
            "zh-Hant",
            "runtimes\\freebsd",
            "runtimes\\linux",
            "runtimes\\linux-arm",
            "runtimes\\linux-arm64",
            "runtimes\\linux-musl-x64",
            "runtimes\\linux-x64",
            "runtimes\\osx",
            "runtimes\\osx-x64",
            "runtimes\\unix",
            "runtimes\\win7-x64",
            "runtimes\\win7-x86",
            "runtimes\\win8-x64",
            "runtimes\\win8-x86",
            "runtimes\\win81-x64",
            "runtimes\\win81-x86",
            "runtimes\\win-arm",
            "runtimes\\win-arm64",
            "ref" };
        
        Information("Removing " + removeFolder.Length + " folders from '" + binFolder + "'...");
        DeleteDirectories(removeFolder.Select(p => System.IO.Path.Combine(binFolder, p)), new DeleteDirectorySettings {
            Recursive = true,
            Force = true
        });
    });

Task("Copy artifacts")
    .IsDependentOn("Clean output folder")
    .Does(() => 
    {
        Information("Copying artifacts...");
        CopyDirectory("./artifacts", binFolder);
        
        Information("Copying MSIX meta data...");
        CopyDirectory("./msix", binFolder);
    });

Task("Sign files")
    .WithCriteria(shouldSign)
    .IsDependentOn("Publish .NET Framework")
    .IsDependentOn("Publish .NET Core")
    .IsDependentOn("Trim publish folder")
    .IsDependentOn("Copy artifacts")
    .Does(() =>
    {
        var allFiles = 
            (System.IO.Directory.EnumerateFiles(binFolder, "*msix*.dll")).Union
            (System.IO.Directory.EnumerateFiles(binFolder, "*msix*.exe")).Union
            (System.IO.Directory.EnumerateFiles(binFolder, "External.*.dll")).Union
            (System.IO.Directory.EnumerateFiles(System.IO.Path.Combine(binFolder, "dgss"), "msixhero*.exe")).Union
            (System.IO.Directory.EnumerateFiles(System.IO.Path.Combine(binFolder, "templates"), "AppAttach*.ps1")).Union
            (System.IO.Directory.EnumerateFiles(System.IO.Path.Combine(binFolder, "scripts"), "*.ps1")).ToArray();

        Information("Signing " + allFiles.Length + " files with signtool.exe...");
        StartProcess(
            System.IO.Path.Combine(binFolder, "redistr", "sdk", "x64", "signtool.exe"),
                new ProcessSettings {
                    Arguments = "sign /n \"Marcin Otorowski\" /t http://time.certum.pl/ /fd sha256 /d \"MSIX Hero\" /v " + string.Join(" ", allFiles.Select(s => "\"" + s + "\"")),
                    RedirectStandardOutput = false
                });
    });

Task("Build MSIX")
    .IsDependentOn("Prepare MSIX")
    .IsDependentOn("Publish .NET Framework")
    .IsDependentOn("Publish .NET Core")
    .IsDependentOn("Trim publish folder")
    .IsDependentOn("Copy artifacts")
    .Does(() => {
        StartProcess(
            System.IO.Path.Combine(binFolder, "msixherocli.exe"),
                new ProcessSettings {
                    Arguments = "pack -d \"" + binFolder + "\" -p \"" + System.IO.Path.Combine(msixFolder, "msix-hero-" + version + ".msix") + "\"",
                    RedirectStandardOutput = false
                });
    });

Task("Sign MSIX")
    .WithCriteria(shouldSign)
    .IsDependentOn("Build MSIX")
    .Does(() => {
        StartProcess(
            System.IO.Path.Combine(binFolder, "msixherocli.exe"),
                new ProcessSettings {
                    Arguments = "sign \"" + System.IO.Path.Combine(msixFolder, "msix-hero-" + version + ".msix") + "\"",
                    RedirectStandardOutput = false
                });
    });

Task("Prepare MSIX")
    .IsDependentOn("Trim publish folder")
    .IsDependentOn("Sign files")
    .Does(() => {
        Information("Updating manifest file '" + System.IO.Path.Combine(binFolder, "AppxManifest.xml") + "' with version '" + version + "'...");
        var file = System.IO.File.ReadAllText(System.IO.Path.Combine(binFolder, "AppxManifest.xml"));
        file = System.Text.RegularExpressions.Regex.Replace(file, "<Identity Name=\"MSIXHero\" Version=\"([0-9\\.]+)\"", "<Identity Name=\"MSIXHero\" Version=\"" + version + "\"");
        System.IO.File.WriteAllText(System.IO.Path.Combine(binFolder, "AppxManifest.xml"), file);
    });

RunTarget(target);