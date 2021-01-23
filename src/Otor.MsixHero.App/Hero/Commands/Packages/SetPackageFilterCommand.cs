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

using Otor.MsixHero.App.Hero.Commands.Base;
using Otor.MsixHero.Infrastructure.Configuration;

namespace Otor.MsixHero.App.Hero.Commands.Packages
{
    public class SetPackageFilterCommand : UiCommand
    {
        public SetPackageFilterCommand()
        {
        }

        public SetPackageFilterCommand(
            PackageFilter filter, 
            string searchKey)
        {
            this.Filter = filter;
            this.SearchKey = searchKey;
        }

        public PackageFilter Filter { get; set; }
        
        public string SearchKey { get; set; }
    }
}
