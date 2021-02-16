# 2.0.64
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
See https://msixhero.net/2021/02/07/msix-hero-2-0-new-features-ui-open-source/

### Improvements ###
* Improved dependency viewer with legend and better captions (#86)
* Add fallback color to acryllic windows (301edbd3)
* Removed "Preview" branding and changed the identity to public (f01bd937)
* Some minor UI improvements (f632a678)
* All changes from 2.0.0 (alpha) and 2.0.39 (beta)

### Bug fixes ###
* (Regression) Fix crash to desktop when grouping packages by install date (#84)

# 2.0.39 (beta) #
See https://msixhero.net/2021/02/01/msix-hero-2-0-39-beta/

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
See https://msixhero.net/2020/12/10/msix-hero-2-0-preview/ for more details

### New features ###
* New UI
* The Dashboard view
* Ability to start any entry point
* Command-line generator
* Sorting and grouping by installation date
* New Event Viewer
* Other changes
