[![Build status](https://ci.appveyor.com/api/projects/status/qg51ctiga8ada0ib?svg=true)](https://ci.appveyor.com/project/marcinotorowski/msix-hero)

## About
MSIX Hero is the ultimate toolkit, which integrates several other tools, Software Development Kits and PowerShell scripts in a single, elegant UI.
[![](https://msixhero.net/wp-content/uploads/2020/02/05-dependencies-150x150.png)](https://msixhero.net/#header_footer_builder)

## Tech stack
The project is written almost exclusively in .NET 3.1. Smaller parts use .NET Framework 4.7 and PowerShell.

## Building
Run `build.ps1` to start the build. This starts a cake script that will restore Nuget packages, build all necessary projects, clean-up and copy results to the `/out` subfolder.

**Note:** The official binaries and MSIX setup available on http://msixhero.net are additionally signed. Appveyor CI is not signing them, but you can use another script `build-and-sign.ps1` to build and sign. You may be required to configure the selected signature first, as the defaults values are not meant to work out-of-the-box.