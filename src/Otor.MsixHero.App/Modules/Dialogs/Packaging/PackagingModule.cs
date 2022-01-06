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

using Otor.MsixHero.App.Controls;
using Otor.MsixHero.App.Modules.Dialogs.Packaging.ModificationPackage.View;
using Otor.MsixHero.App.Modules.Dialogs.Packaging.ModificationPackage.ViewModel;
using Otor.MsixHero.App.Modules.Dialogs.Packaging.Names.View;
using Otor.MsixHero.App.Modules.Dialogs.Packaging.Names.ViewModel;
using Otor.MsixHero.App.Modules.Dialogs.Packaging.Pack.View;
using Otor.MsixHero.App.Modules.Dialogs.Packaging.Pack.ViewModel;
using Otor.MsixHero.App.Modules.Dialogs.Packaging.Unpack.View;
using Otor.MsixHero.App.Modules.Dialogs.Packaging.Unpack.ViewModel;
using Prism.Ioc;
using Prism.Modularity;

namespace Otor.MsixHero.App.Modules.Dialogs.Packaging
{
    public class PackagingModule : IModule
    {
        public void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterDialog<ModificationPackageView, ModificationPackageViewModel>(NavigationPaths.DialogPaths.PackagingModificationPackage);
            containerRegistry.RegisterDialog<PackView, PackViewModel>(NavigationPaths.DialogPaths.PackagingPack);
            containerRegistry.RegisterDialog<NamesView, NamesViewModel>(NavigationPaths.DialogPaths.PackagingNames);
            containerRegistry.RegisterDialog<UnpackView, UnpackViewModel>(NavigationPaths.DialogPaths.PackagingUnpack);
        }

        public void OnInitialized(IContainerProvider containerProvider)
        {
        }
    }
}
