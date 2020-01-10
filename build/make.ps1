if (Test-Path "$PSScriptRoot\dist")
{
    Remove-Item -Path "$PSScriptRoot\dist" -Force -Recurse;
}

New-Item -Path "$PSScriptRoot\dist" -Force -ItemType Directory | Out-Null;

$version = git describe --long;
if (-not ($version -match "v(\d+\.\d+)\-(\d+)\-g([a-z0-9]+)"))
{
    throw "Unexpected git version $version";
}

$version = $Matches[1] + "." + $Matches[2] + ".0";

dotnet publish "$PSScriptRoot\..\otor.msixhero.sln" -p:AssemblyVersion=$version -p:Version=$version --output "$PSScriptRoot\dist" --configuration Release --nologo --force

Copy-Item "$PSScriptRoot\..\artifacts\*" "$PSScriptRoot\dist" -Recurse | Out-Null;

$toDelete = @(
    "CodeCoverage",
    "cs",
    "de",
    "es",
    "fr",
    "hu",
    "it",
    "ja",
    "ko",
    "pl",
    "pt-br",
    "ro",
    "ru",
    "sv",
    "tr",
    "zh-Hans",
    "zh-Hant",
    "runtimes\freebsd",
    "runtimes\linux",
    "runtimes\linux-arm",
    "runtimes\linux-arm64",
    "runtimes\linux-musl-x64",
    "runtimes\linux-x64",
    "runtimes\osx",
    "runtimes\osx-x64",
    "runtimes\unix",
    "runtimes\win7-x64",
    "runtimes\win7-x86",
    "runtimes\win8-x64",
    "runtimes\win8-x86",
    "runtimes\win81-x64",
    "runtimes\win81-x86",
    "runtimes\win-arm",
    "runtimes\win-arm64",
    "ref",
    "Moq.dll",
    "*.pdb",
    "Microsoft.TestPlatform*",
    "Microsoft.VisualStudio*",
    "nunit*.*",
    "otor.msixhero.lib.tests.dll",
    "testhost*.*");

    foreach ($item in $toDelete)
    {
        Remove-Item "$PSScriptRoot\dist\$item" -Force -Recurse;
    }

    $allFiles = Get-ChildItem -Path "$PSScriptRoot\dist" -Filter *msix*.dll;
    $allFiles += Get-ChildItem -Path "$PSScriptRoot\dist" -Filter *msix*.exe;

    $listOfFiles = "";
    foreach ($item in $allFiles)
    {
        $listOfFiles += " `"" + $item.FullName + "`"";
    }

    & "$PSScriptRoot\dist\redistr\sdk\x64\signtool.exe" sign /n "Marcin Otorowski" /t http://time.certum.pl/ /fd sha256 /d "MSIX Hero" /v $allFiles.FullName