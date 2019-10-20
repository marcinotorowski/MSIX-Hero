using System;
using System.Collections.Generic;
using System.Text;
using MSI_Hero.Modules.Installed.View;
using MSI_Hero.ViewModel;
using Prism.Ioc;
using Prism.Modularity;

namespace MSI_Hero.Modules.Main
{
    public class MainModule : IModule
    {
        public static string Path = "Main";

        public void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterForNavigation<MainView>(Path);
            containerRegistry.RegisterSingleton(typeof(MainViewModel));
        }

        public void OnInitialized(IContainerProvider containerProvider)
        {
        }
    }
}
