using System;
using System.Collections.Generic;
using System.Text;
using MSI_Hero.Modules.Installed.View;
using MSI_Hero.Modules.Installed.ViewModel;
using Prism.Ioc;
using Prism.Modularity;

namespace MSI_Hero.Modules.Installed
{
    public class InstalledModule : IModule
    {
        public static string Path = "Installed";

        public void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterForNavigation<InstalledView>(Path);
            containerRegistry.RegisterSingleton(typeof(InstalledViewModel));
        }

        public void OnInitialized(IContainerProvider containerProvider)
        {
        }
    }
}
