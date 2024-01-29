// MSIX Hero
// Copyright (C) 2024 Marcin Otorowski
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
using Otor.MsixHero.App.Modules.Dialogs.Signing.CertificateExport.View;
using Otor.MsixHero.App.Modules.Dialogs.Signing.CertificateExport.ViewModel;
using Otor.MsixHero.App.Modules.Dialogs.Signing.NewSelfSigned.View;
using Otor.MsixHero.App.Modules.Dialogs.Signing.NewSelfSigned.ViewModel;
using Otor.MsixHero.App.Modules.Dialogs.Signing.PackageSigning.View;
using Otor.MsixHero.App.Modules.Dialogs.Signing.PackageSigning.ViewModel;
using Prism.Ioc;
using Prism.Modularity;

namespace Otor.MsixHero.App.Modules.Dialogs.Signing
{
    public class SigningModule : IModule
    {
        public void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterDialog<CertificateExportView, CertificateExportViewModel>(NavigationPaths.DialogPaths.SigningCertificateExport);
            containerRegistry.RegisterDialog<NewSelfSignedView, NewSelfSignedViewModel>(NavigationPaths.DialogPaths.SigningNewSelfSigned);
            containerRegistry.RegisterDialog<PackageSigningView, PackageSigningViewModel>(NavigationPaths.DialogPaths.SigningPackageSigning);
        }

        public void OnInitialized(IContainerProvider containerProvider)
        {
        }
    }
}
