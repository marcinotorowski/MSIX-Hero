if (Test-Path "$PSScriptRoot\dist") {
    Remove-Item -Path "$PSScriptRoot\dist" -Force -Recurse;
}

New-Item -Path "$PSScriptRoot\dist" -Force -ItemType Directory | Out-Null;

$version = git describe --long;

if ($version -match "v(\d+\.\d+)\-(\d+)\-g([a-z0-9]+)")
{
    $version = $Matches[1] + "." + $Matches[2] + ".0";
}
elseif ($version -match "v(\d+\.\d+)\.(\d+)(?:\.\d+)*\-(\d+)\-g([a-z0-9]+)")
{
    $cnt = [System.Int32]::Parse($Matches[2]) + [System.Int32]::Parse($Matches[3]);
    $version = $Matches[1] + "." + $cnt + ".0";
}
else {
    throw "Unexpected git version $version";
}

dotnet publish "$PSScriptRoot\..\otor.msixhero.sln" -p:AssemblyVersion=$version -p:Version=$version --output "$PSScriptRoot\dist" --configuration Release --nologo --force

Copy-Item "$PSScriptRoot\..\artifacts\*" "$PSScriptRoot\dist" -Recurse | Out-Null;
Copy-Item "$PSScriptRoot\msix\*" "$PSScriptRoot\dist" -Recurse | Out-Null;

$toDelete = @(
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
    "*.pdb");

foreach ($item in $toDelete) {
    if (Test-Path "$PSScriptRoot\dist\$item") {
        Remove-Item "$PSScriptRoot\dist\$item" -Force -Recurse;
    }
    else {
        Write-Warning "File $item does not exist.";
    }
}

$allFiles = Get-ChildItem -Path "$PSScriptRoot\dist" -Filter "*msix*.dll";
$allFiles += Get-ChildItem -Path "$PSScriptRoot\dist" -Filter "*msix*.exe";
$allFiles += Get-ChildItem -Path "$PSScriptRoot\dist" -Filter "External.EricZimmerman.Registry.dll";

$listOfFiles = "";
foreach ($item in $allFiles) {
    $listOfFiles += " `"" + $item.FullName + "`"";
}

& "$PSScriptRoot\dist\redistr\sdk\x64\signtool.exe" sign /n "Marcin Otorowski" /t http://time.certum.pl/ /fd sha256 /d "MSIX Hero" /v $allFiles.FullName

$content = Get-Content "$PSScriptRoot\dist\AppxManifest.xml" -Raw;
$replace = '<Identity Name="MSIXHero" Version="' + $version + '"';
$content = $content -replace '<Identity Name="MSIXHero" Version="([0-9\.]+)"',$replace;
Set-Content -Path "$PSScriptRoot\dist\AppxManifest.xml" -Value $content;

# Create msix package
.\dist\msixherocli.exe pack -d "$PSScriptRoot\dist" -p "$PSScriptRoot\msixhero-$version.msix"
.\dist\msixherocli.exe sign "$PSScriptRoot\msixhero-$version.msix"