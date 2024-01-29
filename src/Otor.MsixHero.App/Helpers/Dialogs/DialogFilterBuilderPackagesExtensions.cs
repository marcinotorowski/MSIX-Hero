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

using System;
using Otor.MsixHero.Appx.Packaging;

namespace Otor.MsixHero.App.Helpers.Dialogs;

public static class DialogFilterBuilderPackagesExtensions
{
    public static DialogFilterBuilder WithPackages(
        this DialogFilterBuilder dialogFilterBuilder, 
        PackageTypes packageTypes = PackageTypes.Packages, 
        int order = 0)
    {
        var hasMsix = packageTypes.HasFlag(PackageTypes.Msix);
        var hasAppx = packageTypes.HasFlag(PackageTypes.Appx);
        var hasMsixBundle = packageTypes.HasFlag(PackageTypes.MsixBundle);
        var hasAppxBundle = packageTypes.HasFlag(PackageTypes.AppxBundle);

        if (hasMsix)
        {
            dialogFilterBuilder.WithExtension(FileConstants.MsixExtension, Resources.Localization.Dialogs_Filter_Packages, order);
        }

        if (hasAppx)
        {
            dialogFilterBuilder.WithExtension(FileConstants.AppxExtension, Resources.Localization.Dialogs_Filter_Packages, order);
        }

        if (hasMsixBundle)
        {
            dialogFilterBuilder.WithExtension(FileConstants.MsixBundleExtension, Resources.Localization.Dialogs_Filter_Bundles, order);
        }

        if (hasAppxBundle)
        {
            dialogFilterBuilder.WithExtension(FileConstants.AppxBundleExtension, Resources.Localization.Dialogs_Filter_Bundles, order);
        }

        return dialogFilterBuilder;
    }

    public static DialogFilterBuilder WithWindowsInstallerFiles(
        this DialogFilterBuilder dialogFilterBuilder, 
        int order = 0)
    {
        dialogFilterBuilder.WithExtension(FileConstants.MsiExtension, Resources.Localization.Dialogs_Filter_Msi, order);
        return dialogFilterBuilder;
    }

    public static DialogFilterBuilder WithExecutableFiles(
        this DialogFilterBuilder dialogFilterBuilder, 
        int order = 0)
    {
        dialogFilterBuilder.WithExtension(FileConstants.ExeExtension, Resources.Localization.Dialogs_Filter_Exe, order);
        return dialogFilterBuilder;
    }

    public static DialogFilterBuilder WithRegistry(
        this DialogFilterBuilder dialogFilterBuilder, 
        int order = 0)
    {
        dialogFilterBuilder.WithExtension(FileConstants.RegExtension, Resources.Localization.Dialogs_Filter_Registry, order);
        return dialogFilterBuilder;
    }

    public static DialogFilterBuilder WithManifests(
        this DialogFilterBuilder dialogFilterBuilder, 
        int order = 0)
    {
        dialogFilterBuilder.WithFile(FileConstants.AppxManifestFile, Resources.Localization.Dialogs_Filter_Manifests, order);
        return dialogFilterBuilder;
    }
    public static DialogFilterBuilder WithWinget(
        this DialogFilterBuilder dialogFilterBuilder, 
        int order = 0)
    {
        dialogFilterBuilder.WithExtension(FileConstants.WingetExtension, Resources.Localization.Dialogs_Filter_Winget, order);
        return dialogFilterBuilder;
    }

    public static DialogFilterBuilder WithCertificates(
        this DialogFilterBuilder dialogFilterBuilder,
        CertificateTypes types = CertificateTypes.Cer | CertificateTypes.Pfx,
        int order = 0)
    {
        if (types.HasFlag(CertificateTypes.Pfx))
        {
            dialogFilterBuilder.WithExtension(FileConstants.PfxExtension, Resources.Localization.Dialogs_Filter_Certificates, order);
        }

        if (types.HasFlag(CertificateTypes.Cer))
        {
            dialogFilterBuilder.WithExtension(FileConstants.CerExtension, Resources.Localization.Dialogs_Filter_Certificates, order);
        }

        return dialogFilterBuilder;
    }

    public static DialogFilterBuilder WithAppInstaller(
        this DialogFilterBuilder dialogFilterBuilder, 
        int order = 0)
    {
        dialogFilterBuilder.WithExtension(FileConstants.AppInstallerExtension, Resources.Localization.Dialogs_Filter_AppInstaller, order);
        return dialogFilterBuilder;
    }

    [Flags]
    public enum PackageTypes
    {
        Msix = 1,
        Appx = 2,
        MsixBundle = 4,
        AppxBundle = 8,
        Packages = Msix | Appx,
        Bundles = MsixBundle | AppxBundle,
        All = Packages | Bundles
    }

    [Flags]
    public enum CertificateTypes
    {
        Pfx = 1,
        Cer = 2
    }
}