// MSIX Hero
// Copyright (C) 2022 Marcin Otorowski
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

using System.Windows.Media;
using Otor.MsixHero.App.Hero.State;
using Otor.MsixHero.App.Mvvm;

namespace Otor.MsixHero.App.Modules.Main.Sidebar.ViewModels
{
    public class SidebarItemViewModel : NotifyPropertyChanged
    {
        private bool isChecked;

        public SidebarItemViewModel(ApplicationMode screen, string name, string title, Geometry icon)
        {
            this.Screen = screen;
            this.Icon = icon;
            this.Name = name;
            this.Title = title;
            this.IsVisible = true;
        }

        public SidebarItemViewModel(ApplicationMode screen, string name)
        {
            this.Screen = screen;
            this.Name = name;
            this.IsVisible = false;
        }

        public bool IsVisible { get; }

        public ApplicationMode Screen { get; }

        public string Name { get; }

        public string Title { get; }

        public Geometry Icon { get; }

        public bool IsChecked
        {
            get => this.isChecked;
            set => this.SetField(ref this.isChecked, value);
        }
    }
}
