// MSIX Hero
// Copyright (C) 2024 Marcin Otorowski
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// Full notice:
// https://github.com/marcinotorowski/msix-hero/blob/develop/LICENSE.md

using System.Windows.Input;

namespace Otor.MsixHero.App.Hero.Commands
{
    public static class MsixHeroRoutedUICommands
    {
        static MsixHeroRoutedUICommands()
        {
            SetVolumeAsDefault = new RoutedUICommand() { Text = "Set volume as default…" };
            Deprovision = new RoutedUICommand { Text = "Deprovision for all users" };
            OpenStore = new RoutedUICommand { Text = "Open Store Product Page" };
            OpenExplorer = new RoutedUICommand { Text = "Open install location", InputGestures = { new KeyGesture(Key.E, ModifierKeys.Control) } };
            OpenExplorerUser = new RoutedUICommand { Text = "Open user data folder", InputGestures = { new KeyGesture(Key.U, ModifierKeys.Control) } };
            OpenManifest = new RoutedUICommand { Text = "Open manifest", InputGestures = { new KeyGesture(Key.M, ModifierKeys.Control )}};
            OpenConfigJson = new RoutedUICommand { Text = "Open config.json", InputGestures = { new KeyGesture(Key.J, ModifierKeys.Control )}};
            StartPackage = new RoutedUICommand { Text = "Run app", InputGestures = { new KeyGesture(Key.Enter, ModifierKeys.Control) } };
            StopPackage = new RoutedUICommand { Text = "Stop app" };
            RunTool = new RoutedUICommand { Text = "Run tool in package context" };
            OpenPowerShell = new RoutedUICommand { Text = "Open PowerShell console" };
            MountRegistry = new RoutedUICommand { Text = "Mount registry" };
            DismountRegistry = new RoutedUICommand { Text = "Dismount registry" };
            MountVolume = new RoutedUICommand { Text = "Mount volume" };
            ChangeVolume = new RoutedUICommand { Text = "Change volume" };
            CheckUpdates = new RoutedUICommand { Text = "Check for updates" };
            DismountVolume = new RoutedUICommand { Text = "Dismount volume" };
            CreateSelfSign = new RoutedUICommand { Text = "Create new self-signed certificate "};
            OpenLogs = new RoutedUICommand { Text = "Show event viewer", InputGestures = { new KeyGesture(Key.E, ModifierKeys.Control) } };
            RemovePackage = new RoutedUICommand { Text = "Remove package" };
            AddPackage = new RoutedUICommand { Text = "Add package", InputGestures = { new KeyGesture(Key.Insert)}};
            OpenAppsFeatures = new RoutedUICommand { Text = "Open Apps and Features" };
            OpenDevSettings = new RoutedUICommand { Text = "Open Developer Settings" };
            OpenResign = new RoutedUICommand { Text = "Sign MSIX package(s)" };
            InstallCertificate = new RoutedUICommand { Text = "Install certificate from a .CER file" };
            ExtractCertificate = new RoutedUICommand { Text = "Extract certificate from an .MSIX file" };
            CertManager = new RoutedUICommand { Text = "Open certificate manager" };
            Pack = new RoutedUICommand { Text = "Packs a package to a directory" };
            UpdateImpact = new RoutedUICommand { Text = "Analyzes update impact" };
            DependencyViewer = new RoutedUICommand { Text = "Analyzes dependencies" };
            Unpack = new RoutedUICommand { Text = "Unpacks a package to a directory" };
            AppInstaller = new RoutedUICommand { Text = "Opens a wizard for .appinstaller generation" };
            SharedPackageContainer = new RoutedUICommand { Text = "Opens a wizard for Shared App Container generation" };
            ModificationPackage = new RoutedUICommand { Text = "Opens a wizard for modification package generation" };
            AppAttach = new RoutedUICommand { Text = "Opens a wizard for app attach generation" };
            Settings = new RoutedUICommand { Text = "Opens settings" };
            PackageExpert = new RoutedUICommand { Text = "Open Package Expert"};
            ServiceManager = new RoutedUICommand { Text = "Open Service Manager"};
            Winget = new RoutedUICommand { Text = "Create winget manifest"};
            OpenFile = new RoutedUICommand { Text = "Open the selected file"};
            ResetContainer = new RoutedUICommand { Text = "Reset container"};
            EditContainer = new RoutedUICommand { Text = "Edit container"};
            AddContainer = new RoutedUICommand { Text = "Reset container"};
        }

        public static RoutedUICommand ResetContainer { get; }

        public static RoutedUICommand EditContainer { get; }

        public static RoutedUICommand AddContainer { get; }

        public static RoutedUICommand SetVolumeAsDefault { get; }
        
        public static RoutedUICommand Deprovision { get; }

        public static RoutedUICommand OpenExplorer { get; }

        public static RoutedUICommand OpenStore { get; }
        
        public static RoutedUICommand OpenResign { get; }

        public static RoutedUICommand OpenExplorerUser { get; }
        
        public static RoutedUICommand CreateSelfSign { get; }

        public static RoutedUICommand ExtractCertificate { get; }

        public static RoutedUICommand CertManager { get; }

        public static RoutedUICommand InstallCertificate { get; }
        
        public static RoutedUICommand OpenManifest { get; }
        
        public static RoutedUICommand OpenFile { get; }

        public static RoutedUICommand OpenConfigJson { get; }

        public static RoutedUICommand RunTool { get; }

        public static RoutedUICommand StartPackage { get; }

        public static RoutedUICommand StopPackage { get; }

        public static RoutedUICommand AddPackage { get; }

        public static RoutedUICommand OpenPowerShell { get; }

        public static RoutedUICommand OpenAppsFeatures { get; }

        public static RoutedUICommand OpenDevSettings { get; }

        public static RoutedUICommand MountRegistry { get; }

        public static RoutedUICommand DismountRegistry { get; }

        public static RoutedUICommand MountVolume { get; }

        public static RoutedUICommand ChangeVolume { get; }

        public static RoutedUICommand CheckUpdates { get; }

        public static RoutedUICommand DismountVolume { get; }

        public static RoutedUICommand OpenLogs { get; }

        public static RoutedUICommand RemovePackage { get; }
        
        public static RoutedUICommand Pack { get; }
        
        public static RoutedUICommand Unpack { get; }

        public static RoutedUICommand UpdateImpact { get; }

        public static RoutedUICommand DependencyViewer { get; }

        public static RoutedUICommand Winget { get; }

        public static RoutedUICommand SharedPackageContainer { get; }

        public static RoutedUICommand AppInstaller { get; }

        public static RoutedUICommand PackageExpert { get; }

        public static RoutedUICommand ModificationPackage { get; }

        public static RoutedUICommand AppAttach { get; }

        public static RoutedUICommand Settings { get; }

        public static RoutedUICommand ServiceManager { get; }
    }
}
