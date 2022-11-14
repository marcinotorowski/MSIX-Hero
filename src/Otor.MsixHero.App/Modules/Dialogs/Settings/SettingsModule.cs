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

using Otor.MsixHero.App.Modules.Dialogs.Settings.Tabs.AppAttach.View;
using Otor.MsixHero.App.Modules.Dialogs.Settings.Tabs.AppAttach.ViewModel;
using Otor.MsixHero.App.Modules.Dialogs.Settings.Tabs.Commands.View;
using Otor.MsixHero.App.Modules.Dialogs.Settings.Tabs.Commands.ViewModel;
using Otor.MsixHero.App.Modules.Dialogs.Settings.Tabs.Editors.View;
using Otor.MsixHero.App.Modules.Dialogs.Settings.Tabs.Editors.ViewModel;
using Otor.MsixHero.App.Modules.Dialogs.Settings.Tabs.Interface.View;
using Otor.MsixHero.App.Modules.Dialogs.Settings.Tabs.Interface.ViewModel;
using Otor.MsixHero.App.Modules.Dialogs.Settings.Tabs.Other.View;
using Otor.MsixHero.App.Modules.Dialogs.Settings.Tabs.Other.ViewModel;
using Otor.MsixHero.App.Modules.Dialogs.Settings.Tabs.Signing.View;
using Otor.MsixHero.App.Modules.Dialogs.Settings.Tabs.Signing.ViewModel;
using Otor.MsixHero.App.Modules.Dialogs.Settings.View;
using Otor.MsixHero.App.Modules.Dialogs.Settings.ViewModel;
using Prism.Ioc;
using Prism.Modularity;
using Prism.Mvvm;
using Prism.Regions;

namespace Otor.MsixHero.App.Modules.Dialogs.Settings
{
    public class SettingsModule : IModule
    {
        public void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterDialog<SettingsView, SettingsViewModel>(NavigationPaths.DialogPaths.Settings);
        }

        public void OnInitialized(IContainerProvider containerProvider)
        {
            var regionManager = containerProvider.Resolve<IRegionManager>();

            regionManager.RegisterViewWithRegion("TabSigning", typeof(SigningSettingsTabView));
            regionManager.RegisterViewWithRegion("TabCommands", typeof(CommandsSettingsTabView));
            regionManager.RegisterViewWithRegion("TabAppAttach", typeof(AppAttachSettingsTabView));
            regionManager.RegisterViewWithRegion("TabOther", typeof(OtherSettingsTabView));
            regionManager.RegisterViewWithRegion("TabInterface", typeof(InterfaceSettingsTabView));
            regionManager.RegisterViewWithRegion("TabEditor", typeof(EditorSettingsTabView));

            ViewModelLocationProvider.Register<SigningSettingsTabView, SigningSettingsTabViewModel>();
            ViewModelLocationProvider.Register<CommandsSettingsTabView, CommandsSettingsTabViewModel>();
            ViewModelLocationProvider.Register<AppAttachSettingsTabView, AppAttachSettingsTabViewModel>();
            ViewModelLocationProvider.Register<OtherSettingsTabView, OtherSettingsTabViewModel>();
            ViewModelLocationProvider.Register<InterfaceSettingsTabView, InterfaceSettingsTabViewModel>();
            ViewModelLocationProvider.Register<EditorSettingsTabView, EditorSettingsTabViewModel>();
        }
    }
}
