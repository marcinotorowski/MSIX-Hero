## About

MSIX Hero is an open source MSIX manager and toolkit. The list of features is available here: https://msixhero.net/features/

![Screenshot](https://msixhero.net/msixhero.png)

Bug reports, feature improvements and pull requests are welcome.

## Tech stack
The project is written almost exclusively in .NET 6.0. Minor parts use .NET Framework 4.7 and PowerShell.

## Official binaries ###
You can download official binaries (signed + deployable via MSIX/appinstaller) from the official website: https://msixhero.net/get

## Building
The built process is straightforward. You can simply open `Otor.MsixHero.sln`, restore nuget packages and run the project. Alternatively, use the provided build script `build.ps1` to start the build. This starts a cake script that restores Nuget packages, builds all necessary projects, performs a clean-up and copies results to the `/out` subfolder. For each assembly, the build script takes the major and minor version from the last tag (pattern vMAJOR.MINOR) and the third unit comes from number of commits since that tag (starting with 0).

**Note:** The official binaries and MSIX setup available on http://msixhero.net are additionally signed. AppVeyor CI is not signing them, but you can use another script `build-and-sign.ps1` to build and sign. Adjust the signature detail first, as the defaults values are not meant to work out-of-the-box, you can also use the parameter --CertName for this to work, in this case the command to build may look like this:

    dotnet cake build.cake --ScriptArgs -CertName="ABC"

## Build status ##
Branches *develop* and *master* and pull requests are built automatically using Appveyor CI.

[![Build status](https://ci.appveyor.com/api/projects/status/ukp54g7jnwa3g177?svg=true)](https://ci.appveyor.com/project/marcinotorowski/msix-hero-bj0mu)

## License ##
* GNU General Public License v3
* https://github.com/marcinotorowski/MSIX-Hero/blob/develop/LICENSE.md
* https://msixhero.net/license/
