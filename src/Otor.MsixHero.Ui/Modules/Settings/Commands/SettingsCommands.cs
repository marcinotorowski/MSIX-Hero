﻿using System.Windows.Input;

namespace Otor.MsixHero.Ui.Modules.Settings.Commands
{
    public static class SettingsCommands
    {
        public static RoutedUICommand OpenIcon = new RoutedUICommand { Text = "Browse for an icon..." };
        public static RoutedUICommand DeleteIcon = new RoutedUICommand { Text = "Remove icon...", InputGestures = { new KeyGesture(Key.Delete)} };
    }
}