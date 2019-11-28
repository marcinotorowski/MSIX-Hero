using System;
using System.Collections.Generic;
using System.Text;
using otor.msixhero.ui.Modules.Dialogs.EventViewer.View;
using otor.msixhero.ui.Modules.Dialogs.EventViewer.ViewModel;
using otor.msixhero.ui.Modules.Dialogs.NewSelfSigned.View;
using otor.msixhero.ui.Modules.Dialogs.NewSelfSigned.ViewModel;
using otor.msixhero.ui.Modules.Dialogs.PackageSigning.View;
using otor.msixhero.ui.Modules.Dialogs.PackageSigning.ViewModel;
using otor.msixhero.ui.Modules.PackageList.View;
using otor.msixhero.ui.Modules.PackageList.ViewModel;
using Prism.Ioc;
using Prism.Modularity;

namespace otor.msixhero.ui.Modules.Dialogs
{
    public class DialogsModule : IModule
    {
        public static string NewSelfSignedPath = "NewSelfSigned";
        public static string EventViewerPath = "EventViewer";
        public static string PackageSigningPath = "PackageSigning";

        public void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterDialog<NewSelfSignedView, NewSelfSignedViewModel>(NewSelfSignedPath);
            containerRegistry.RegisterDialog<EventViewerView, EventViewerViewModel>(EventViewerPath);
            containerRegistry.RegisterDialog<PackageSigningView, PackageSigningViewModel>(PackageSigningPath);
        }

        public void OnInitialized(IContainerProvider containerProvider)
        {
        }
    }
}
