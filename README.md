## About

MSIX Hero is an open source MSIX manager and toolkit. The list of features is available here: https://msixhero.net/features/

![Screenshot](https://msixhero.net/wp-content/uploads/2020/12/image-1024x659.png)

Bug reports, feature improvements and pull requests are welcome.

## Tech stack
The project is written almost exclusively in .NET Core 3.1. Smaller parts use .NET Framework 4.7 and PowerShell.

## Official binaries ###
You can download official binaries (signed + deployable via MSIX/appinstaller) from the official website: https://msixhero.net/download

## Building
The built process is straightforward. You can simply open `Otor.MsixHero.sln`, restore nuget packages and run the project. Alternatively, use the provided build script `build.ps1` to start the build. This starts a cake script that restores Nuget packages, builds all necessary projects, performas a clean-up and copies results to the `/out` subfolder.

**Note:** The official binaries and MSIX setup available on http://msixhero.net are additionally signed. Appveyor CI is not signing them, but you can use another script `build-and-sign.ps1` to build and sign. Adjuste the signature detail first, as the defaults values are not meant to work out-of-the-box.

## Build status ##
Branches *develop* and *master* are built automatically using AppVeyor CI.

[![Build status](https://ci.appveyor.com/api/projects/status/qg51ctiga8ada0ib?svg=true)](https://ci.appveyor.com/project/marcinotorowski/msix-hero)