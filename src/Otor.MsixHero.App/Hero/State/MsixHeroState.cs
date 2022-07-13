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

namespace Otor.MsixHero.App.Hero.State
{
    public class MsixHeroState
    {
        public MsixHeroState()
        {
            this.Packages = new PackagesState();
            this.Volumes = new VolumesState();
            this.Tools = new ToolsState();
            this.EventViewer = new EventViewerState();
        }

        public ApplicationMode CurrentMode { get; set; }

        public PackagesState Packages { get; set; }

        public VolumesState Volumes { get; set; }

        public ToolsState Tools { get; set; }

        public EventViewerState EventViewer { get; set; }
    }
}
