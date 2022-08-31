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

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using Otor.MsixHero.App.Helpers.Interop;
using Otor.MsixHero.App.Hero;
using Otor.MsixHero.App.Hero.Commands;
using Otor.MsixHero.App.Hero.Commands.Packages;
using Otor.MsixHero.App.Hero.Events;
using Otor.MsixHero.App.Hero.Events.Base;
using Otor.MsixHero.App.Modules.PackageManagement.PackageList.ViewModels;
using Otor.MsixHero.App.Mvvm.Converters;
using Otor.MsixHero.Appx.Diagnostic.RunningDetector;
using Otor.MsixHero.Infrastructure.Configuration;
using Otor.MsixHero.Infrastructure.Localization;
using Otor.MsixHero.Infrastructure.Services;
using Prism.Events;

namespace Otor.MsixHero.App.Modules.PackageManagement.PackageList.Views
{
    /// <summary>
    /// Interaction logic for PackagesListView.
    /// </summary>
    public partial class PackagesListView
    {
        private readonly IMsixHeroApplication _application;
        private readonly IConfigurationService _configService;
        private IList<MenuItem> _tools;

        private readonly ObservableCollection<SelectableInstalledPackageViewModel> _allItems = new ObservableCollection<SelectableInstalledPackageViewModel>();
        private readonly ICollectionView _allItemsView;

        public PackagesListView(IMsixHeroApplication application, IConfigurationService configService)
        {
            this._application = application;
            this._configService = configService;

            this._application.EventAggregator.GetEvent<ToolsChangedEvent>().Subscribe(_ => this._tools = null);
            this._application.EventAggregator.GetEvent<UiExecutedEvent<SelectPackagesCommand>>().Subscribe(this.OnSelectPackagesCommand, ThreadOption.UIThread);
            this._application.EventAggregator.GetEvent<UiExecutedEvent<GetPackagesCommand>>().Subscribe(this.OnGetPackagesCommand, ThreadOption.UIThread);
            this._application.EventAggregator.GetEvent<UiExecutedEvent<SetPackageSortingCommand>>().Subscribe(this.OnSetPackageSorting, ThreadOption.UIThread);
            this._application.EventAggregator.GetEvent<UiExecutedEvent<SetPackageFilterCommand>>().Subscribe(this.OnSetPackageFilter, ThreadOption.UIThread);
            this._application.EventAggregator.GetEvent<UiExecutedEvent<SetPackageGroupingCommand>>().Subscribe(this.OnSetPackageGrouping, ThreadOption.UIThread);
            this._application.EventAggregator.GetEvent<PubSubEvent<ActivePackageFullNames>>().Subscribe(this.OnActivePackageFullNamesChanged, ThreadOption.UIThread);

            this.InitializeComponent();

            this._allItemsView = CollectionViewSource.GetDefaultView(this._allItems);
            this._allItemsView.Filter = row => ((SelectableInstalledPackageViewModel)row).IsVisible;

            this.ListBox.ItemsSource = this._allItemsView;
            this.ListBox.SelectionChanged += this.ListBoxOnSelectionChanged;

            MsixHeroTranslation.Instance.CultureChanged += (_, _) =>
            {
                if (Dispatcher.CheckAccess())
                {
                    if (this.ListBox.SelectedItems.Count != 1)
                    {
                        return;
                    }

                    var current = (SelectableInstalledPackageViewModel)this.ListBox.SelectedItems[0];

                    if (current == null)
                    {
                        return;
                    }

                    this._application.CommandExecutor.Invoke(this, new SelectPackagesCommand());
                    this._application.CommandExecutor.Invoke(this, new SelectPackagesCommand(current.PackageFullName));
                }
                else
                {
                    Dispatcher.BeginInvoke(() =>
                    {
                        if (this.ListBox.SelectedItems.Count != 1)
                        {
                            return;
                        }

                        var current = (SelectableInstalledPackageViewModel)this.ListBox.SelectedItems[0];

                        if (current == null)
                        {
                            return;
                        }

                        this._application.CommandExecutor.Invoke(this, new SelectPackagesCommand());
                        this._application.CommandExecutor.Invoke(this, new SelectPackagesCommand(current.PackageFullName));
                    });
                }
            };

            this.ListBox.PreviewKeyDown += ListBoxOnKeyDown;
            this.ListBox.PreviewKeyUp += ListBoxOnKeyUp;
        }

        private void ListBoxOnKeyDown(object sender, KeyEventArgs e)
        {
            // This is to ensure that we de-bounce the request if the user keeps pressing a navigation button...
            if (e.Key == Key.Down || e.Key == Key.Up || e.Key == Key.PageDown || e.Key == Key.PageUp)
            {
                this.ListBox.SelectionChanged -= this.ListBoxOnSelectionChanged;
            }
        }

        private void ListBoxOnKeyUp(object sender, KeyEventArgs e)
        {
            // This is to ensure that we de-bounce the request if the user keeps pressing a navigation button...
            if (e.Key == Key.Down || e.Key == Key.Up || e.Key == Key.PageDown || e.Key == Key.PageUp)
            {
                this.ListBox.SelectionChanged += this.ListBoxOnSelectionChanged;
                this._application.CommandExecutor.Invoke(this, new SelectPackagesCommand(this.ListBox.SelectedItems.OfType<InstalledPackageViewModel>().Select(p => p.PackageFullName)));
            }
        }

        private void OnActivePackageFullNamesChanged(ActivePackageFullNames obj)
        {
            if (this._application.ApplicationState.Packages.Filter.HasFlag(PackageFilter.Running))
            {
                this._allItemsView.Refresh();
            }
        }

        private void OnGetPackagesCommand(UiExecutedPayload<GetPackagesCommand> obj)
        {
            try
            {
                this.ListBox.SelectionChanged -= this.ListBoxOnSelectionChanged;
                this._allItems.Clear();

                foreach (var item in ((PackagesListViewModel)this.DataContext).AllPackages)
                {
                    this._allItems.Add(item);
                }

                var selectedPackages = new HashSet<string>(((PackagesListViewModel)this.DataContext).SelectedPackages.Select(p => p.PackageFullName));

                switch (selectedPackages.Count)
                {
                    case 0:
                        this.ListBox.SelectedItems.Clear();
                        break;
                    
                    case 1:
                        this.ListBox.SelectedItem = this._allItems.FirstOrDefault(item => selectedPackages.Contains(item.PackageFullName));
                        break;

                    default:
                        foreach (var item in this._allItems)
                        {
                            if (selectedPackages.Contains(item.PackageFullName))
                            {
                                this.ListBox.SelectedItems.Add(item);
                            }
                            else
                            {
                                this.ListBox.SelectedItems.Remove(item);
                            }
                        }

                        break;
                }
            }
            finally
            {
                this.ListBox.SelectionChanged += this.ListBoxOnSelectionChanged;
            }

            if (!this._allItemsView.SortDescriptions.Any())
            {
                this.SetSortingAndGrouping();
            }
        }

        private void OnSetPackageGrouping(UiExecutedPayload<SetPackageGroupingCommand> obj)
        {
            this.SetSortingAndGrouping();
        }

        private void OnSetPackageFilter(UiExecutedPayload<SetPackageFilterCommand> obj)
        {
            this._allItemsView.Refresh();
        }

        private void OnSetPackageSorting(UiExecutedPayload<SetPackageSortingCommand> obj)
        {
            this.SetSortingAndGrouping();
        }

        private void SetSortingAndGrouping()
        {
            var currentSort = this._application.ApplicationState.Packages.SortMode;
            var currentSortDescending = this._application.ApplicationState.Packages.SortDescending;
            var currentGroup = this._application.ApplicationState.Packages.GroupMode;
            
            using (((ICollectionView)this.ListBox.ItemsSource).DeferRefresh())
            {
                string sortProperty;
                string groupProperty;

                switch (currentSort)
                {
                    case PackageSort.Name:
                        sortProperty = nameof(SelectableInstalledPackageViewModel.DisplayName);
                        break;
                    case PackageSort.Publisher:
                        sortProperty = nameof(SelectableInstalledPackageViewModel.DisplayPublisherName);
                        break;
                    case PackageSort.Architecture:
                        sortProperty = nameof(SelectableInstalledPackageViewModel.Architecture);
                        break;
                    case PackageSort.InstallDate:
                        sortProperty = nameof(SelectableInstalledPackageViewModel.InstallDate);
                        break;
                    case PackageSort.Type:
                        sortProperty = nameof(SelectableInstalledPackageViewModel.Type);
                        break;
                    case PackageSort.Version:
                        sortProperty = nameof(SelectableInstalledPackageViewModel.Version);
                        break;
                    case PackageSort.PackageType:
                        sortProperty = nameof(SelectableInstalledPackageViewModel.DisplayPackageType);
                        break;
                    default:
                        sortProperty = null;
                        break;
                }

                switch (currentGroup)
                {
                    case PackageGroup.None:
                        groupProperty = null;
                        break;
                    case PackageGroup.Publisher:
                        groupProperty = nameof(SelectableInstalledPackageViewModel.DisplayPublisherName);
                        break;
                    case PackageGroup.Architecture:
                        groupProperty = nameof(SelectableInstalledPackageViewModel.Architecture);
                        break;
                    case PackageGroup.InstallDate:
                        groupProperty = nameof(SelectableInstalledPackageViewModel.InstallDate);
                        break;
                    case PackageGroup.Type:
                        groupProperty = nameof(SelectableInstalledPackageViewModel.Type);
                        break;
                    default:
                        return;
                }

                // 1) First grouping
                if (groupProperty == null)
                {
                    this._allItemsView.GroupDescriptions.Clear();
                }
                else
                {
                    var pgd = this._allItemsView.GroupDescriptions.OfType<PropertyGroupDescription>().FirstOrDefault();
                    if (pgd == null || pgd.PropertyName != groupProperty)
                    {
                        this._allItemsView.GroupDescriptions.Clear();

                        if (groupProperty == nameof(SelectableInstalledPackageViewModel.InstallDate))
                        {
                            this._allItemsView.GroupDescriptions.Add(new PropertyGroupDescription(groupProperty, GroupDateConverter.Instance));
                        }
                        else
                        {
                            this._allItemsView.GroupDescriptions.Add(new PropertyGroupDescription(groupProperty));
                        }
                    }
                }

                // 2) Then sorting
                if (sortProperty == null)
                {
                    this._allItemsView.SortDescriptions.Clear();
                    this._allItemsView.SortDescriptions.Add(new SortDescription(nameof(SelectableInstalledPackageViewModel.HasStar), ListSortDirection.Descending));
                }
                else
                {
                    this._allItemsView.SortDescriptions.Clear();
                    this._allItemsView.SortDescriptions.Add(new SortDescription(nameof(SelectableInstalledPackageViewModel.HasStar), ListSortDirection.Descending));
                    this._allItemsView.SortDescriptions.Add(new SortDescription(sortProperty, currentSortDescending ? ListSortDirection.Descending : ListSortDirection.Ascending));
                }

                if (this._allItemsView.GroupDescriptions.Any())
                {
                    var gpn = ((PropertyGroupDescription)this._allItemsView.GroupDescriptions[0]).PropertyName;
                    if (this._allItemsView.GroupDescriptions.Any() && this._allItemsView.SortDescriptions.All(sd => sd.PropertyName != gpn))
                    {
                        this._allItemsView.SortDescriptions.Insert(0, new SortDescription(gpn, ListSortDirection.Ascending));
                    }
                }
            }
        }

        private void OnSelectPackagesCommand(UiExecutedPayload<SelectPackagesCommand> obj)
        {
            var selectedPackages = new HashSet<string>(obj.Request.SelectedFullNames);

            this.ListBox.SelectionChanged -= this.ListBoxOnSelectionChanged;
            try
            {
                switch (selectedPackages.Count)
                {
                    case 0:
                        this.ListBox.SelectedItems.Clear();
                        break;

                    case 1:
                        this.ListBox.SelectedItem = this._allItems.FirstOrDefault(item => selectedPackages.Contains(item.PackageFullName));
                        break;

                    default:
                    {
                        foreach (var item in this._allItems)
                        {
                            if (selectedPackages.Contains(item.PackageFullName))
                            {
                                this.ListBox.SelectedItems.Add(item);
                            }
                            else
                            {
                                this.ListBox.SelectedItems.Remove(item);
                            }
                        }

                        break;
                    }
                }
            }
            finally
            {
                this.ListBox.SelectionChanged += this.ListBoxOnSelectionChanged;
            }
        }

        private void ListBoxOnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            this._application.CommandExecutor.Invoke(this, new SelectPackagesCommand(this.ListBox.SelectedItems.OfType<SelectableInstalledPackageViewModel>().Select(p => p.PackageFullName)));
        }

        private void PackageContextMenu_OnContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            if (this._tools != null)
            {
                return;
            }

            this.SetTools();
            var frameworkElement = (FrameworkElement)sender;
            // ReSharper disable once PossibleNullReferenceException
            var lastMenu = frameworkElement.ContextMenu.Items.OfType<MenuItem>().Last();

            lastMenu.Items.Clear();
            foreach (var item in this._tools)
            {
                lastMenu.Items.Add(item);
            }

            lastMenu.Items.Add(new Separator());
            lastMenu.Items.Add(new MenuItem
            {
                Command = MsixHeroRoutedUICommands.Settings,
                CommandParameter = "tools",
                Header = MsixHero.App.Resources.Localization.Packages_MoreCommands
            });
        }

        private void SetTools()
        {
            if (this._tools != null)
            {
                return;
            }

            this._tools = new List<MenuItem>();
            var configuredTools = this._configService.GetCurrentConfiguration().Packages.Tools;

            foreach (var item in configuredTools ?? Enumerable.Empty<ToolListConfiguration>())
            {
                this._tools.Add(new MenuItem
                {
                    Command = MsixHeroRoutedUICommands.RunTool,
                    Icon = new Image { Source = WindowsIcons.GetIconFor(string.IsNullOrEmpty(item.Icon) ? item.Path : item.Icon) },
                    Header = item.Name,
                    CommandParameter = item
                });
            }
        }
    }
}
