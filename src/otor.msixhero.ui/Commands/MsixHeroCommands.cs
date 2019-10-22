using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;

namespace MSI_Hero.Commands
{
    public static class MsixHeroCommands
    {
        static MsixHeroCommands()
        {
            OpenExplorer = new RoutedUICommand { Text = "Open install location", InputGestures = { new KeyGesture(Key.E, ModifierKeys.Control) } };
            OpenExplorerUser = new RoutedUICommand { Text = "Open user data folder", InputGestures = { new KeyGesture(Key.U, ModifierKeys.Control) } };
            OpenManifest = new RoutedUICommand { Text = "Open manifest", InputGestures = { new KeyGesture(Key.M, ModifierKeys.Control )}};
            RunApp = new RoutedUICommand { Text = "Run app", InputGestures = { new KeyGesture(Key.Enter, ModifierKeys.Control) } };
            RunTool = new RoutedUICommand { Text = "Run tool in package context" };
            OpenPowerShell = new RoutedUICommand { Text = "Open PowerShell console" };
            MountRegistry = new RoutedUICommand { Text = "Mount registry" };
            UnmountRegistry = new RoutedUICommand { Text = "Unmount registry" };
        }

        public static RoutedUICommand OpenExplorer { get; }

        public static RoutedUICommand OpenExplorerUser { get; }

        public static RoutedUICommand OpenManifest { get; }

        public static RoutedUICommand RunTool { get; }

        public static RoutedUICommand RunApp { get; }

        public static RoutedUICommand OpenPowerShell { get; }

        public static RoutedUICommand MountRegistry { get; }

        public static RoutedUICommand UnmountRegistry { get; }
    }
}
