using System;
using System.Collections.Generic;
using System.Text;
using otor.msixhero.ui.Modules.Dialogs.NewSelfSigned.View;
using otor.msixhero.ui.Modules.Dialogs.NewSelfSigned.ViewModel;
using otor.msixhero.ui.Modules.PackageList.View;
using otor.msixhero.ui.Modules.PackageList.ViewModel;
using Prism.Ioc;
using Prism.Modularity;

namespace otor.msixhero.ui.Modules.Dialogs
{
    public class DialogsModule : IModule
    {
        public static string NewSelfSignedPath = "NewSelfSigned";

        public void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterDialog<NewSelfSignedView, NewSelfSignedViewModel>(NewSelfSignedPath);
            containerRegistry.RegisterSingleton(typeof(PackageListViewModel));
        }

        public void OnInitialized(IContainerProvider containerProvider)
        {
        }
    }
}
