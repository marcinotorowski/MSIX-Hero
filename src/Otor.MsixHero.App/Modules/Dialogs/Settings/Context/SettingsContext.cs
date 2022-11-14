﻿// MSIX Hero
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

using System;
using System.Collections.Generic;
using Otor.MsixHero.App.Modules.Dialogs.Settings.Tabs;

namespace Otor.MsixHero.App.Modules.Dialogs.Settings.Context
{
    public class SettingsContext : ISettingsContext
    {
        private readonly List<ISettingsComponent> _changeableObjects = new List<ISettingsComponent>();

        public void Register(ISettingsComponent changeable)
        {
            this._changeableObjects.Add(changeable);
            this.ChangeableRegistered?.Invoke(this, changeable);
        }

        public IReadOnlyCollection<ISettingsComponent> ChangeableObjects => this._changeableObjects;
        
        public EventHandler<ISettingsComponent> ChangeableRegistered;
    }
}
