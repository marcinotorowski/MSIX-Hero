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

var shouldSign = HasArgument("Sign");    
var shouldTest = !HasArgument("SkipTest");
var certName = HasArgument("CertName") ? Argument<string>("CertName") : "Marcin Otorowski";

Task("Clean output folder")
    .Does(() =>
{
    Information("Cleaning folder '" + outFolder + "'...");
    CleanDirectory(outFolder);
});

Task("Build")
    .IsDependentOn("Clean output folder")
    .IsDependentOn("Test")
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

    if (p != 0)
    {
        throw new InvalidOperationException("GIT.EXE failed with exit code " + p);
    }

    var v = string.Join("\r\n", redirectedStandardOutput);
    Information("> " + v);

    var r = System.Text.RegularExpressions.Regex.Match(v, @"^v(?<major>\d+)\.(?<minor>\d+)(?:\.(?<revision>\d+))?.*-(?<offset>\d+)-g(?<commit>[a-fA-F0-9]+)$");   
    if (!r.Success)
    {        
        throw new Exception("Unexpected git version " + v);
    }

    int revision = 0;
    if (r.Groups["revision"].Success)
    {
        revision = int.Parse(r.Groups["revision"].Value);
    }

    revision += int.Parse(r.Groups["offset"].Value);
    version = r.Groups["major"].Value + "." + r.Groups["minor"].Value + "." + revision + ".0";

    Information("Product version is '" + version + "'");
    
    if (BuildSystem.AppVeyor.IsRunningOnAppVeyor)
    {
        if (BuildSystem.AppVeyor.Environment.PullRequest.IsPullRequest)
        {   
            // For Pull Requests, add some artificial ending
            BuildSystem.AppVeyor.UpdateBuildVersion(version + "-pr" + BuildSystem.AppVeyor.Environment.PullRequest.Number);            
        }
        else
        {
            BuildSystem.AppVeyor.UpdateBuildVersion(version);
        }

    }
});

Task("Test")
    .IsDependentOn("Determine version")
    .Does(() => {           

        if (!shouldTest)
        {
            Information("Skipping tests due to the presence of the --SkipTest switch.");
            return;
        }

        var projectDir = System.IO.Path.Combine("src", "Otor.MsixHero.Tests");
        var unitTestCsProj = System.IO.Path.Combine(projectDir, "Otor.MsixHero.Tests.csproj");
        var publishDir = System.IO.Path.Combine(projectDir, "bin", "PublishCore", "net8.0-windows");
        var unitTestDll = System.IO.Path.Combine(publishDir, "Otor.MsixHero.Tests.dll");
        
        Information("Building unit tests...");        
        DotNetCoreBuild(unitTestCsProj, new DotNetCoreBuildSettings()
        {
            Configuration = configuration,
            OutputDirectory = publishDir
        });
        
        Information("Executing unit tests..."); 
        var testSettings = new DotNetCoreTestSettings();
        testSettings.Filter = "TestCategory!=Integration";

        if (BuildSystem.AppVeyor.IsRunningOnAppVeyor)
        {
            testSettings.Logger = "Appveyor;LogFileName=test-result.trx";
        }

        DotNetCoreTest(unitTestDll, testSettings);
        /*
        var resultsFile = System.IO.Path.Combine("TestResults", "test-result.trx");
        Information("Results has been saved in " + resultsFile);
        if (BuildSystem.AppVeyor.IsRunningOnAppVeyor)
        {
            Information("Updating AppVeyor test results...");
            BuildSystem.AppVeyor.UploadTestResults(resultsFile, AppVeyorTestResultsType.NUnit3);
        } */       
    });

Task("Publish .NET Core")
    .IsDependentOn("Test")
    .IsDependentOn("Clean output folder")
    .IsDependentOn("Determine version")
    .IsDependentOn("Copy artifacts")
    .Does(() =>
{
    Information("Publishing solution...");
    var settings = new DotNetCorePublishSettings
    {
        Configuration = configuration,
        OutputDirectory = binFolder
    };

    settings.MSBuildSettings = new DotNetCoreMSBuildSettings()
    .WithProperty("AssemblyVersion", version)
    .WithProperty("Version", version)
    .WithProperty("Copyright",  "(C) 2021 by Marcin Otorowski")
    .WithProperty("ErrorOnDuplicatePublishOutputFiles", "false");
    DotNetCorePublish("./Otor.MsixHero.sln", settings);
});

Task("Trim publish folder")
    .IsDependentOn("Publish .NET Core")
    .Does(() => {

        Information("Deleting all PDB files from '" + binFolder + "'...");
        DeleteFiles(System.IO.Path.Combine(binFolder, "*.pdb"));
        
        var removeFolder = new string[] {
            "cs",
            "es",
            "fr",
            "it",
            "ja",
            "ko",
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
            "runtimes\\win81-x64",
            "runtimes\\win81-x86",
            "runtimes\\win-arm",
            "runtimes\\win-arm64",
            "ref" };
        
        Information("Removing " + removeFolder.Length + " folders from '" + binFolder + "'...");
        
        DeleteDirectories(removeFolder.Where(p => System.IO.Directory.Exists(System.IO.Path.Combine(binFolder, p))).Select(p => System.IO.Path.Combine(binFolder, p)), new DeleteDirectorySettings {
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
    .IsDependentOn("Publish .NET Core")
    .IsDependentOn("Trim publish folder")
    .IsDependentOn("Copy artifacts")
    .Does(() =>
    {
        var allFiles = 
            (System.IO.Directory.EnumerateFiles(binFolder, "*msix*.dll")).Union
            (System.IO.Directory.EnumerateFiles(binFolder, "*msix*.exe")).Union
            (System.IO.Directory.EnumerateFiles(binFolder, "External.*.dll")).Union
            (System.IO.Directory.EnumerateFiles(System.IO.Path.Combine(binFolder, "templates"), "AppAttach*.ps1")).Union
            (System.IO.Directory.EnumerateFiles(System.IO.Path.Combine(binFolder, "scripts"), "*.ps1")).ToArray();

        Information("Signing " + allFiles.Length + " files with signtool.exe...");
        var p = StartProcess(
            System.IO.Path.Combine(binFolder, "redistr", "sdk", "x64", "signtool.exe"),
                new ProcessSettings {
                    Arguments = "sign /n \"" + certName + "\" /t http://time.certum.pl/ /fd sha256 /d \"MSIX Hero\" /v " + string.Join(" ", allFiles.Select(s => "\"" + s + "\"")),
                    RedirectStandardOutput = false
                });

        if (p != 0)
        {
            throw new InvalidOperationException("SIGNTOOL.EXE failed with exit code " + p);
        }
    });

Task("Build MSIX")
    .IsDependentOn("Prepare MSIX")
    .IsDependentOn("Publish .NET Core")
    .IsDependentOn("Trim publish folder")
    .IsDependentOn("Copy artifacts")
    .Does(() => {
        var p = StartProcess(
            System.IO.Path.Combine(binFolder, "msixherocli.exe"),
                new ProcessSettings {
                    Arguments = "pack -d \"" + binFolder + "\" -p \"" + System.IO.Path.Combine(msixFolder, "msix-hero-" + version + ".msix") + "\"",
                    RedirectStandardOutput = false
                });

        if (p != 0)
        {
            throw new InvalidOperationException("MSIXHEROCLI.EXE failed with exit code " + p);
        }
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
        file = System.Text.RegularExpressions.Regex.Replace(file, "<Identity Name=\"([^\"]*)\" Version=\"([0-9\\.]+)\"", "<Identity Name=\"$1\" Version=\"" + version + "\"");        
        System.IO.File.WriteAllText(System.IO.Path.Combine(binFolder, "AppxManifest.xml"), file);
    });

RunTarget(target);