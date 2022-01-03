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

using System;

namespace Otor.MsixHero.App.Mvvm.Changeable
{
    public interface IChangeableValue : IChangeable
    {
        /// <summary>
        /// An event fired when the value was changed.
        /// </summary>
        event EventHandler<ValueChangedEventArgs> ValueChanged;

        /// <summary>
        /// An event fired when the value is about to be changed.
        /// </summary>
        event EventHandler<ValueChangingEventArgs> ValueChanging;
    }
}