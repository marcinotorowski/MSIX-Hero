// MSIX Hero
// Copyright (C) 2021 Marcin Otorowski
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

using Otor.MsixHero.Appx.Diagnostic.Developer.Enums;
using Otor.MsixHero.Appx.Exceptions;

namespace Otor.MsixHero.Appx.Diagnostic.Developer;

public abstract class BaseWindowsSideloadingConfigurator : ISideloadingConfigurator
{
    public abstract SideloadingStatus Get();

    public abstract bool Set(SideloadingStatus status);

    public abstract SideloadingFlavor Flavor { get; }

    public virtual void AssertSideloadingEnabled()
    {
        if (this.Get() == SideloadingStatus.NotAllowed)
        {
            throw new DeveloperModeException("Developer mode or sideloading must be enabled to install packages outside of Microsoft Store.");
        }
    }

    public virtual void AssertDeveloperModeEnabled()
    {
        if (this.Get() < SideloadingStatus.DeveloperMode)
        {
            throw new DeveloperModeException("Developer mode must be enabled to use this feature.");
        }
    }
}