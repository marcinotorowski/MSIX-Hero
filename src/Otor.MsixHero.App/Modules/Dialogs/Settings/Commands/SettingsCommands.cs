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

namespace Otor.MsixHero.App.Modules.Dialogs.Settings.Commands
{
    public static class SettingsCommands
    {
        public static RoutedUICommand OpenIcon = new RoutedUICommand { Text = Resources.Localization.Dialogs_Settings_BrowseIcon };
        public static RoutedUICommand DeleteIcon = new RoutedUICommand { Text = Resources.Localization.Dialogs_Settings_RemoveIcon, InputGestures = { new KeyGesture(Key.Delete)} };
    }
}
