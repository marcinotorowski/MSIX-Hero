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
            this.SelectedManifestPaths = new List<string>();
        }

        public SelectPackagesCommand(string manifestPaths, PackageSelectionMode mode = PackageSelectionMode.Replace)
        {
            this.SelectionMode = mode;
            if (manifestPaths == null)
            {
                this.SelectedManifestPaths = new List<string>();
            }
            else
            {
                this.SelectedManifestPaths = new List<string> { manifestPaths };
            }
        }

        public SelectPackagesCommand(IList<string> manifestPaths)
        {
            this.SelectionMode = PackageSelectionMode.Replace;
            this.SelectedManifestPaths = manifestPaths;
        }

        public SelectPackagesCommand(params string[] manifestPaths)
        {
            this.SelectionMode = PackageSelectionMode.Replace;
            this.SelectedManifestPaths = manifestPaths.ToList();
        }

        public SelectPackagesCommand(IEnumerable<string> manifestPaths)
        {
            this.SelectionMode = PackageSelectionMode.Replace;
            this.SelectedManifestPaths = manifestPaths.ToList();
        }

        public IList<string> SelectedManifestPaths { get; }

        public PackageSelectionMode SelectionMode { get; set; }
    }
}
