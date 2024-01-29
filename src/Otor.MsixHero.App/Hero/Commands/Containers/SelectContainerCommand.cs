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

using MediatR;
using Otor.MsixHero.Appx.Packaging.SharedPackageContainer.Entities;

namespace Otor.MsixHero.App.Hero.Commands.Containers
{
    public class SelectSharedPackageContainerCommand : IRequest
    {
        public SelectSharedPackageContainerCommand()
        {
        }

        public SelectSharedPackageContainerCommand(SharedPackageContainer container)
        {
            this.SelectedContainer = container;
        }

        public SharedPackageContainer SelectedContainer { get; set; }
    }
}
