## About

MSIX Hero is an open source MSIX manager and toolkit. The feature list is available here: https://msixhero.net/features/

![MSIX Hero - Screenshot](https://github.com/marcinotorowski/MSIX-Hero/blob/develop/screenshot.png?raw=true)

Bug reports, feature requests and pull requests are welcome.

## Requirements
* .NET 8.0
* Windows 10 1809 or later / Windows 11

## Official binaries ###
You can download official binaries (signed + deployable via MSIX) from the official website: https://msixhero.net/get

## Build
## Requirements
* Visual Studio 2022
* .NET 8
* Windows SDK 10.0.22621.0

### How to Build
The build process is simple. You can simply open `Otor.MsixHero.sln`, restore the nuget packages and run the project. Alternatively, you can use the included build script `build.ps1` to start the build. This starts a cake script that restores nuget packages, builds all necessary projects, performs a cleanup and copies the results to the `/out` subdirectory. For each assembly, the build script takes the major and minor version from the last tag (pattern `vMAJOR.MINOR`) and the third unit comes from the number of commits since that tag (starting with 0).

**Note:** The official binaries and MSIX setup available at http://msixhero.net are additionally signed. AppVeyor CI does not sign them, but you can use another script `build-and-sign.ps1` to build and sign them. Adjust the signature details first, as the defaults are not meant to work out of the box, you can also use the --CertName parameter to make it work, in this case the command to build may look like this

    dotnet cake build.cake --ScriptArgs -CertName="ABC"

## Build Status ##
Branches *develop* and *master* and pull requests are automatically built using Appveyor CI.

[![Build Status](https://ci.appveyor.com/api/projects/status/ukp54g7jnwa3g177?svg=true)](https://ci.appveyor.com/project/marcinotorowski/msix-hero-bj0mu)

## License ##
* GNU General Public License v3
* https://github.com/marcinotorowski/MSIX-Hero/blob/develop/LICENSE.md
* https://msixhero.net/license/