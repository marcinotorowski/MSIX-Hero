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

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Otor.MsixHero.App.Hero;
using Otor.MsixHero.App.Hero.Commands.Packages;
using Otor.MsixHero.App.Mvvm;
using Otor.MsixHero.App.Mvvm.Progress;
using Otor.MsixHero.Appx.Packaging.Installation.Entities;
using Otor.MsixHero.Appx.Packaging.Services;
using Prism.Commands;

namespace Otor.MsixHero.App.Modules.Dialogs.Packaging.SharedPackageContainer.ViewModel;

public class InstalledPackages : NotifyPropertyChanged
{
    private bool _listRefreshRequired = true;
    private readonly SharedPackageContainerViewModel _parent;
    private readonly IMsixHeroApplication _application;
    private bool _isVisible;
    private SearchableInstallPackage _selectedItem;
    private string _searchKey;

    public InstalledPackages(
        SharedPackageContainerViewModel parent, 
        IMsixHeroApplication application,
        IBusyManager busyManager)
    {
        this._parent = parent;
        this._application = application;
        this.Packages = new AsyncProperty<ObservableCollection<SearchableInstallPackage>>();
            
        this.Accept = new DelegateCommand(this.OnAccept, this.CanAccept);
        this.Cancel = new DelegateCommand(this.OnCancel);
        this.Refresh = new DelegateCommand(this.OnRefresh);

        parent.Packages.Changed += PackagesOnChanged;
        busyManager.StatusChanged += BusyManagerOnStatusChanged;
    }

    private void BusyManagerOnStatusChanged(object sender, IBusyStatusChange e)
    {
        this.Packages.Progress.Progress = e.Progress;
        this.Packages.Progress.Message = e.Message;
        this.Packages.Progress.IsLoading = e.IsBusy;
    }

    public AsyncProperty<ObservableCollection<SearchableInstallPackage>> Packages { get; }

    public SearchableInstallPackage SelectedItem
    {
        get => this._selectedItem;
        set => this.SetField(ref this._selectedItem, value);
    }

    private async Task<ObservableCollection<SearchableInstallPackage>> GetInstalledApps()
    {
        IList<InstalledPackage> srcPackages = this._application.ApplicationState.Packages.AllPackages;

        if (srcPackages?.Any() != true)
        {
            srcPackages = await this._application.CommandExecutor.Invoke<GetPackagesCommand, IList<InstalledPackage>>(this, new GetPackagesCommand(PackageFindMode.CurrentUser)).ConfigureAwait(false);
        }

        var currentSelection = new HashSet<string>(this._parent.Packages.Select(p => p.FamilyName.CurrentValue), StringComparer.Ordinal);

        var list = new ObservableCollection<SearchableInstallPackage>(srcPackages.Select(p =>
        {
            var pkg = new SearchableInstallPackage(p);
            SetVisibility(pkg, this._searchKey);
            SetAvailability(pkg, currentSelection);
            return pkg;
        }));

        this._listRefreshRequired = false;
        return list;
    }

    public string SearchKey
    {
        get => this._searchKey;
        set
        {
            var onlyOnVisible = this._searchKey != null && !string.IsNullOrEmpty(value) && value.Length >= this._searchKey.Length && this._searchKey.StartsWith(value, StringComparison.OrdinalIgnoreCase);

            if (!this.SetField(ref this._searchKey, value) || this.Packages.CurrentValue == null)
            {
                return;
            }

            foreach (var p in this.Packages.CurrentValue)
            {
                SetVisibility(p, value, onlyOnVisible);
            }
        }
    }

    private static void SetAvailability(SearchableInstallPackage pkg, HashSet<string> disallowedPackageFamilyNames)
    {
        pkg.IsEnabled = !disallowedPackageFamilyNames.Contains(pkg.PackageFamilyName);
    }

    private static void SetVisibility(SearchableInstallPackage pkg, string searchKey, bool onlyOnVisible = false)
    {
        if (!pkg.IsVisible && onlyOnVisible)
        {
            return;
        }

        if (string.IsNullOrEmpty(searchKey))
        {
            pkg.IsVisible = true;
        }
        else
        {
            if (pkg.DisplayName?.Contains(searchKey, StringComparison.OrdinalIgnoreCase) == true)
            {
                pkg.IsVisible = true;
            }
            else if (pkg.DisplayPublisherName?.Contains(searchKey, StringComparison.OrdinalIgnoreCase) == true)
            {
                pkg.IsVisible = true;
            }
            else
            {
                pkg.IsVisible = false;
            }
        }
    }

    public bool IsVisible
    {
        get => this._isVisible;
        set
        {
            if (!this.SetField(ref this._isVisible, value))
            {
                return;
            }

            if (value && this._listRefreshRequired)
            {
                this.OnRefresh();
            }
        }
    }

    public ICommand Accept { get; }

    public ICommand Cancel { get; }

    public ICommand Refresh { get; }

    private void PackagesOnChanged(object sender, EventArgs e)
    {
        this._listRefreshRequired = true;
    }

    private bool CanAccept() => this.SelectedItem != null;

    private void OnAccept()
    {
        this._listRefreshRequired = true;
        this.IsVisible = false;

        var pkg = SharedPackageViewModel.FromInstalledPackage(this.SelectedItem);
        this._parent.Packages.Add(pkg);
    }

    private void OnCancel()
    {
        this._listRefreshRequired = true;
        this.IsVisible = false;
    }

    private void OnRefresh()
    {
        this._listRefreshRequired = false;
        var _ = this.Packages.Load(this.GetInstalledApps());
    }
}