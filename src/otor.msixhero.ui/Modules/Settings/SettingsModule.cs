using System;
using System.Collections.Generic;
using System.Text;
using MSI_Hero.Modules.Settings.View;
using MSI_Hero.Modules.Settings.ViewModel;
using Prism.Ioc;
using Prism.Modularity;

namespace MSI_Hero.Modules.Settings
{
    public class SettingsModule : IModule
    {
        public static string Path = "Settings";

        public void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterDialog<SettingsView, SettingsViewModel>(Path);
            containerRegistry.RegisterSingleton(typeof(SettingsViewModel));
        }

        public void OnInitialized(IContainerProvider containerProvider)
        {
        }
    }
}
