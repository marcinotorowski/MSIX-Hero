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

using System;

namespace Otor.MsixHero.App.Mvvm.Changeable
{
    public abstract class ValueChangedEventArgsBase : EventArgs
    {
        public ValueChangedEventArgsBase(object oldValue, object newValue, object originalValue, bool wasDirty, bool wasTouched)
        {
            this.OldValue = oldValue;
            this.NewValue = newValue;
            this.OriginalValue = originalValue;
            this.WasDirty = wasDirty;
            this.WasTouched = wasTouched;
        }

        public object OldValue { get; private set; }

        public object NewValue { get; private set; }

        public object OriginalValue { get; private set; }

        public bool WasDirty { get; private set; }

        public bool WasTouched { get; private set; }
    }
}