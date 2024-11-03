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
using System.Collections.Generic;
using System.Linq;

namespace Otor.MsixHero.App.Hero.Commands.Packages
{
    public class SelectPackagesCommand : IRequest
    {
        public enum PackageSelectionMode
        {
            Replace,
            Add,
            Remove,
            Toggle
        }
        
        public SelectPackagesCommand()
        {
            this.SelectionMode = PackageSelectionMode.Replace;
            this.SelectedIds = new List<PackageLUID>();
        }

        public SelectPackagesCommand(PackageLUID packageId, PackageSelectionMode mode = PackageSelectionMode.Replace)
        {
            this.SelectionMode = mode;
            this.SelectedIds = new List<PackageLUID> { packageId };
        }

        public SelectPackagesCommand(IList<PackageLUID> packageIds)
        {
            this.SelectionMode = PackageSelectionMode.Replace;
            this.SelectedIds = packageIds;
        }

        public SelectPackagesCommand(params PackageLUID[] packageFullNames)
        {
            this.SelectionMode = PackageSelectionMode.Replace;
            this.SelectedIds = packageFullNames.ToList();
        }

        public SelectPackagesCommand(IEnumerable<PackageLUID> packageFullNames)
        {
            this.SelectionMode = PackageSelectionMode.Replace;
            this.SelectedIds = packageFullNames.ToList();
        }

        public IList<PackageLUID> SelectedIds { get; }

        public PackageSelectionMode SelectionMode { get; set; }
    }
}
