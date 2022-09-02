# 3.0.0 (planned)
Feature highlights:
* New UI with card-based layout
* Multi-language support (English, German and Polish in this release)
* Ability to pin apps in the list view
* Improved package overview sidebar
  * More human friendly display of PSF content
  * PSF content now shows more details, including the content of launch scripts and custom JSON configuration
  * Build meta-data is now displayed
  * Added detection of Host ID
  * File count and total size is now displayed
  * Ability to open dependency packages
* Function buttons (Start app, open installaction folder) are now also displayed in package dialog
* Restyled Event viewer
* New tab ("Advanced") in the app installer editor
* Internal notifications using Windows style
* General improvements, stability and bug fixes

# 2.3.0
Feature highlights:
* New command line verb ```edit``` for unattended MSIX editing. (#78)
  * Changing of package identity (```msixherocli.exe edit <path> setIdentity …```)
  * Setting properties (```msixherocli.exe edit <path> setProperties …```)
  * Adding standard, restricted or custom capability (```msixherocli.exe edit <path> addCapability …```)
  * Setting build meta-data (```msixherocli.exe edit <path> setBuildMetaData …```)
  * Deleting a file (```msixherocli.exe edit <path> deleteFile …```)
  * Adding or replacing a file (```msixherocli.exe edit <path> addFile …```)
  * Setting registry keys (```msixherocli.exe edit <path> setRegistryKey …```)
  * Setting registry values (```msixherocli.exe edit <path> setRegistryValue …```)
  * Deleting registry keys (```msixherocli.exe edit <path> deleteRegistryKey …```)
  * Deleting registry values (```msixherocli.exe edit <path> deleteRegistryValue …```)
  * Importing registry keys from file or local registry (```msixherocli.exe edit <path> importRegistry …```)
  * Bulk editing and chaining (```msixherocli.exe edit <path> list …```)
  * Call ```msixherocli.exe edit <path> --help``` for extensive help for all options. The ```<path>``` in all above commands can be a manifest, MSIX or an unpacked package directory.
* Appinstaller editor dialog now supports package dependencies. (#131)
* In signing-related dialogs, timestamp server can be picked from a predefined list. (#129)    
* New dialog to calculate package full name, publisher hash and PFN (4a93e4a5)

Improvements
* Ability to filter package list by publisher hash or raw name/publisher. (950b5148)
* Ability to skip the validation during packing (in both UI and command-line). (df57c79f)
* Ability to remove unpacked folder after packing it to MSIX file. (2b9a6657)
* Ability to remove MSIX file after unpacking it to a folder (in both UI and command-line). (2b9a6657)
* Ability to copy package family name from the context menu (0f51c9e5)
* Tim Mangan's PsfTooling and RayPack Express are now recognized and shown in the System Summary screen. (9143d703, 5023015b)
* New visual style of the tool dashboard. (6da9433)
* Reworked running app indicator, which now works in real time and has some other improvements. (af59eb7d)
* Better validation of package name, package version and resource ID in many UI dialogs (30ec7bd6)
* Update impact dialog has now icons and better captions for duplicated files. (3e8bbb67, 32d0c5ce)

Resolved issues
* Fixed sideloading and developer settings UI on Windows 10 (2004 and higher) and Windows 11. (#125)   
* Fixed handling of timestamp-related exceptions from ```signtool.exe```. (4e18658)

Technical changes and SDK updates
* Updated ```msixmgr``` (MSIX Core) to version 1.1.92. (b00746c1)
* Migrated MSIX Hero to .NET 6.0. (e546a32e)
* Reduced application size thanks to deprecation of older .NET Framework code. (4cd18578)
* Updated MSIX/Windows SDK to 10.0.22000.0. (2269bf46)


# 2.2.34
See http://msixhero.net/redirect/release-notes/2.2.34 for more details

* When signing or changing an existing MSIX package, the original build metadata will be now preserved (previous version were removing any custom attributes and/or pre-existing values) (#120)
* Fixed visual glitches related to acrylic backgrounds on maximized screen (4fdc7a77)
* Several internal changes and code improvements (2dfc024 and 2790978)

# 2.2.29
See http://msixhero.net/redirect/release-notes/2.2.29 for more details

* It is now possible to pack a folder without a manifest
* Windows 11 is now correctly recognized in the package properties dialog (0cd4f1bf)
* Ability to activate verbose logging + jump to logs folder (Settings screen)
* Fixed App Attach generation with message 'Successfully started the Shell Hardware Detection Service' (#118)
* Fixed ``NullReferenceException`` when searching the list and when certain packages are present on the system (#119)
* General stability improvements

# 2.2.0
See http://msixhero.net/redirect/release-notes/2.2.0 for more details

### New features and highlights ###

* Winget editor has now more options and supports manifest format v1 (#110) 
* Ability to view/edit package files and registry items (#111)
* App attach now supports bulk conversion and additional formats: VHDX and CIM (#113)

### Other changed and improvements ###
* New navigation structure of the Package Properties panel (#111)
* The option to mount a registry has been removed – the new Registry control makes it obsolete (9eb4961c).
* Improved parsing of errors reported by makeappx.exe (a02b4ac9).
* Windows 10 21H1 is now correctly recognized by its marketing name (May 2021 Update) in various places (52b44688).
* Improved UTF-8 handling in saved files (a02b4ac9).
* Minor UI improvements.

### Resolved issues ###
* Fixed incorrect first-time validation of package signing settings, where the information about missing PFX files was shown (ca38766f).
* Fix the button to open Store page that was available for non-store apps (bb7ef0f3).
* Fixed ``NullReferenceException`` when copying install or user-profile path from the OPEN flyout (2f7bdf98).

# 2.1.0
See http://msixhero.net/redirect/release-notes/2.1.0 for more details

### New features and highlights ###
* Support for appinstaller optional and related packages in the App Installer editor (595dfde3)
* It is now possible to control the level of animations and other UI-effects (f9478c80)
* Better performance on low-level/virtual machines or remote sessions (f9478c80)

### Improvements ###
* New dashboard view (273889ed)
* In case of errors when installing packages, the message shown in the UI is more precise (89414f43)
* It is now possible to add multiple files at once in the Sign Package dialog (19e65748)
* Minor UI changes and improvements (e234aa33, c7a543fe)
* Package selector now supports the `.msixbundle` extension (4b4f1dfb)
* Improved parsing of packages (84a9db44)

### Resolved issues ###
* Fixed error when installing a bundle package (a6fe9cf2, #10)
* Fixed Package URL not being saved after changing its value in the app installer editor (67197ec0)
* Fixed missing parameter `--directory` in app attach dialog (f4030d67)
* Fixed a problem with the Add Folder prompt, which did not support cancelling (a2f49a3b)

# 2.0.68

### Improvements ###
* Better UI messages for winget CLI validation (#97)

### Bug fixes ###
* Fix wrong owner of various message boxes (#96)
* Fix wrong checking for winget presence (#97)
* Fix "Parameter set cannot be resolved" when calling cert-related functions interoping with PS (#98)

# 2.0.64
See http://msixhero.net/redirect/release-notes/2.0.64 for more details

### Improvements ###
* Added context menu item to open the *Change Volume* dialog for the selected package (18ad7af8)
* Rewritten *Update Impact* analyzer (#94)
  * Added duplication and bar layout views
  * Added CLI support via `updateImpact` verb
  * Improved performance, especially for big packages
  * Ability to export to XML and start a new comparison
* Minor UI changes (ef8ecc0d)
  * Changed colors of invalid input fields from red to yellow
  * Validation icons are consistent (tab items, input texts, dialogs, the settings view)
  * Tab items have now bigger padding
  * Added extra help with tooltip in the *Modification Package* section, checkbox *Copy to VFS*

### Bug fixes ###
* Fixed a problem with the partition drop-down, which was not showing any value in the *Change Volume* dialog (#90)
* Fixed a problem with invisible text on Windows 10 with dark mode enabled (#93)
* Fixed broken CLI verb `dependencies` (f244efde)
* Fixed a problem with file overview in the *Update impact* dialog, which was not correct for packages having uncompressed Appx blocks (for example ZIP files) (#94)
* Fixed a problem with *Modification Package* dialog, where the checkbox "Copy to VFS" was not reacting to changes of other controls (#92)

# 2.0.46
See http://msixhero.net/redirect/release-notes/2.0.46 for more details

### Improvements ###
* Improved dependency viewer with legend and better captions (#86)
* Add fallback color to acryllic windows (301edbd3)
* Removed "Preview" branding and changed the identity to public (f01bd937)
* Some minor UI improvements (f632a678)
* All changes from 2.0.0 (alpha) and 2.0.39 (beta)

### Bug fixes ###
* (Regression) Fix crash to desktop when grouping packages by install date (#84)

# 2.0.39 (beta) #
See http://msixhero.net/redirect/release-notes/2.0.39 for more details

### Breaking changes ###
* MSIX Hero migrated from .NET Core 3.1 to .NET 5.0 (#74)
* Device Guard version 1 signing is not available anymore (#72)

### Improvements ###
* Raw package name / publisher name is now shown instead of blanks if the translation from PRI failed (#82)
* "Copy to clipboard" button now available for event logs (#79)
* Compacted header in the package view (#73)
* Make it possible to drop a project onto the main window and have it opened (#73)
* Restored buttons and key bindings to open MSIX packages in a new dialog (1.x functionality) (#73)
* Certificate selectors now display icons (3c33658c)
* Improved dialog for import/extract of certificates (4dfcd7bc)

### Resolved issues ###
* Broken deserialization of YAML files (#81)
* Broken certificate validation in the settings screen for default values (98cc77dd)
* Crash to desktop when trying to copy an item in the Packages screen (5498e2d0)
* Progress bar in Package Signing dialog was not stacked for multiple packages (#71)
* Missing scrollbar in event viewer (7b9e2161)
* Fix transparent background during loading (280a2d62)

# 2.0.0 (alpha) #
See http://msixhero.net/redirect/release-notes/2.0.0 for more details

### New features ###
* New UI
* The Dashboard view
* Ability to start any entry point
* Command-line generator
* Sorting and grouping by installation date
* New Event Viewer
* Other changes