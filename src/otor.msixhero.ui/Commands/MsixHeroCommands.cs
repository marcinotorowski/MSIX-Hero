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
            OpenExplorer = new OpenExplorerCommand { Text = "Open folder", InputGestures = { new KeyGesture(Key.E, ModifierKeys.Control) } };
            OpenManifest = new OpenManifestCommand { Text = "Open manifest", InputGestures = { new KeyGesture(Key.M, ModifierKeys.Control )}};
            RunApp = new RunAppCommand { Text = "Run app", InputGestures = { new KeyGesture(Key.Enter, ModifierKeys.Control) } };
            RunTool = new RunToolCommand { Text = "Run tool in package context" };
        }

        public static OpenExplorerCommand OpenExplorer { get; }

        public static OpenManifestCommand OpenManifest { get; }

        public static RunToolCommand RunTool { get; }

        public static RunAppCommand RunApp { get; }
    }
}
