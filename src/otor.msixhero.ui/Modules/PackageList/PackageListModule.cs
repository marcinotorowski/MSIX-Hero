using System;
using System.Collections.Generic;
using System.Text;
using MSI_Hero.Modules.Installed.View;
using MSI_Hero.Modules.Installed.ViewModel;
using Prism.Ioc;
using Prism.Modularity;

namespace MSI_Hero.Modules.Installed
{
    public class PackageListModule : IModule
    {
        public static string Path = "PackageList";

        public void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterForNavigation<PackageListView>(Path);
            containerRegistry.RegisterSingleton(typeof(PackageListViewModel));
        }

        public void OnInitialized(IContainerProvider containerProvider)
        {
        }
    }
}
