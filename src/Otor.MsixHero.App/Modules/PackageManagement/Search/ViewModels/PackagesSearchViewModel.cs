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

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Input;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Otor.MsixHero.App.Hero;
using Otor.MsixHero.App.Hero.Commands.Packages;
using Otor.MsixHero.App.Hero.Events;
using Otor.MsixHero.App.Hero.Events.Base;
using Otor.MsixHero.App.Hero.Executor;
using Otor.MsixHero.App.Mvvm;
using Otor.MsixHero.App.Mvvm.Progress;
using Otor.MsixHero.Appx.Packaging;
using Otor.MsixHero.Appx.Packaging.Services;
using Otor.MsixHero.Infrastructure.Configuration;
using Otor.MsixHero.Infrastructure.Services;
using Prism.Commands;
using Prism.Events;

namespace Otor.MsixHero.App.Modules.PackageManagement.Search.ViewModels
{
    public class PackagesSearchViewModel : NotifyPropertyChanged
    {
        private readonly IMsixHeroApplication _application;
        private readonly IBusyManager _busyManager;
        private readonly IInteractionService _interactionService;
        private readonly IConfigurationService _configurationService;
        private bool _isAllUsers;
        private SourceViewModel _selectedSource;
        private bool _showAddSourcesButton;

        public PackagesSearchViewModel(
            IEventAggregator eventAggregator,
            IMsixHeroApplication application,
            IBusyManager busyManager,
            IInteractionService interactionService,
            IConfigurationService configurationService)
        {
            this._application = application;
            this._busyManager = busyManager;
            this._interactionService = interactionService;
            _configurationService = configurationService;
            this._application.EventAggregator.GetEvent<UiExecutedEvent<SetPackageFilterCommand>>().Subscribe(this.OnSetPackageFilterCommand);
            this._isAllUsers = application.ApplicationState.Packages.Mode.Type == PackageQuerySourceType.InstalledForAllUsers;

            eventAggregator.GetEvent<UiExecutedEvent<GetPackagesCommand>>().Subscribe(this.OnGetPackages);
            eventAggregator.GetEvent<UiFailedEvent<GetPackagesCommand>>().Subscribe(this.OnGetPackages);
            eventAggregator.GetEvent<UiCancelledEvent<GetPackagesCommand>>().Subscribe(this.OnGetPackages);
            eventAggregator.GetEvent<CustomPackageDirectoriesChangedEvent>().Subscribe(this.OnCustomPackageDirsChanged, ThreadOption.UIThread);

            this.Sources.Add(new SourceViewModel(PackageQuerySource.InstalledForCurrentUser(), "Current user"));
            this.Sources.Add(new SourceViewModel(PackageQuerySource.InstalledForAllUsers(), "All users"));
            this.Sources.Add(new SourceViewModel(PackageQuerySource.FromFolder(null), "Folder selected by user"));
            this._selectedSource = this.Sources.FirstOrDefault();

            this.LoadCustomDirs(true);

            this.Browse = new DelegateCommand<object>(p =>
            {
                var selected = (SourceViewModel)p;
                if (selected == null)
                {
                    return;
                }

                if (!this._interactionService.SelectFolder(out var dir))
                {
                    return;
                }

                selected.SourceType = PackageQuerySource.FromFolder(dir);

                this.SelectedSource = selected;
                this.LoadContext(selected.SourceType);
            }, p => ((SourceViewModel)p)?.SourceType.Type == PackageQuerySourceType.Directory);
        }
        
        public string SearchKey
        {
            get => this._application.ApplicationState.Packages.SearchKey;
            set => this._application.CommandExecutor.Invoke(this, new SetPackageFilterCommand(this._application.CommandExecutor.ApplicationState.Packages.Filter, value));
        }

        public bool IsAllUsers
        {
            get => this._isAllUsers;
            set
            {
                if (!this.SetField(ref this._isAllUsers, value))
                {
                    return;
                }

                this.LoadContext(value ? PackageQuerySource.InstalledForAllUsers() : PackageQuerySource.InstalledForCurrentUser());
            }
        }

        public ObservableCollection<SourceViewModel> Sources { get; } = new();

        public SourceViewModel SelectedSource
        {
            get => this._selectedSource;
            set
            {
                if (value.SourceType.Type == PackageQuerySourceType.Directory)
                {
                    if (string.IsNullOrEmpty(value.SourceType.Path))
                    {
                        if (!this._interactionService.SelectFolder(out var dir))
                        {
                            return;
                        }

                        value.SourceType = PackageQuerySource.FromFolder(dir);
                    }
                }

                if (!this.SetField(ref this._selectedSource, value))
                {
                    return;
                }

                this.LoadContext(value.SourceType);
            }
        }

        public PackageQuerySourceType SourceType
        {
            get => this.SelectedSource == null ? PackageQuerySourceType.Installed : this.SelectedSource.SourceType.Type;
            set
            {
                if (this.SelectedSource?.SourceType.Type == value)
                {
                    return;
                }

                this.OnPropertyChanged();

                switch (value)
                {
                    case PackageQuerySourceType.InstalledForAllUsers:
                        this.LoadContext(PackageQuerySource.InstalledForAllUsers());
                        break;
                    case PackageQuerySourceType.InstalledForCurrentUser:
                        this.LoadContext(PackageQuerySource.InstalledForCurrentUser());
                        break;
                }
            }
        }

        public bool ShowAddSourcesButton => this.Sources.Any(d => d.SourceType.Type == PackageQuerySourceType.Directory && d.SourceType.Path != null);

        public ICommand Browse { get; }

        private void LoadCustomDirs(bool initialCall = false)
        {
            // Remove all sources that are directory

            var selectedSource = this.SelectedSource.SourceType;
            var selectedDirectory = this.SelectedSource.SourceType.Type == PackageQuerySourceType.Directory ? this.SelectedSource : null;
            
            for (var i = this.Sources.Count - 1; i >= 0; i--)
            {
                if (this.Sources[i].SourceType.Type != PackageQuerySourceType.Directory)
                {
                    continue;
                }

                this.Sources.RemoveAt(i);
            }

            var customDirs = this._configurationService.GetCurrentConfiguration()?.Packages?.CustomPackageDirectories ?? new List<PackageDirectoryConfiguration>();

            if (!customDirs.Any())
            {
                // Add a dummy entry
                this.Sources.Add(new SourceViewModel(PackageQuerySource.FromFolder(null)));
            }

            foreach (var cd in customDirs)
            {
                this.Sources.Add(new SourceViewModel(PackageQuerySource.FromFolder(cd.Path, cd.IsRecurse), cd.DisplayName ?? Path.GetFileName(cd.Path)));
            }
            
            if (selectedDirectory != null)
            {
                if (customDirs.All(c => c.Path.Resolved != selectedDirectory.SourceType.Path))
                {
                    this.Sources.Add(selectedDirectory);
                }
                else
                {
                    this._selectedSource = this.Sources.FirstOrDefault(s => s.SourceType.Type == selectedDirectory.SourceType.Type && s.SourceType.Path == selectedDirectory.SourceType.Path) ?? this.Sources.FirstOrDefault();
                }
            }
            else
            {
                this._selectedSource = this.Sources.FirstOrDefault(s => s.SourceType.Type == selectedSource.Type && s.SourceType.Path == selectedSource.Path) ?? this.Sources.FirstOrDefault();
            }

            if (!initialCall)
            {
                this.OnPropertyChanged(nameof(this.SelectedSource));
                this.OnPropertyChanged(nameof(this.ShowAddSourcesButton));
            }
        }

        private void OnCustomPackageDirsChanged(IReadOnlyCollection<PackageDirectoryConfiguration> obj)
        {
            this.LoadCustomDirs();
        }

        private void OnGetPackages(UiFailedPayload<GetPackagesCommand> obj)
        {
            this._isAllUsers = this._application.ApplicationState.Packages.Mode.Type == PackageQuerySourceType.InstalledForAllUsers;
            this.OnPropertyChanged(nameof(IsAllUsers));

            var mode = this._application.ApplicationState.Packages.Mode;
            this._selectedSource = this.Sources.FirstOrDefault(s => s.SourceType.Type == mode.Type && s.SourceType.Path == mode.Path) ?? this.Sources.FirstOrDefault();
            this.OnPropertyChanged(nameof(SourceType));
        }

        private void OnGetPackages(UiExecutedPayload<GetPackagesCommand> obj)
        {
            this._isAllUsers = this._application.ApplicationState.Packages.Mode.Type == PackageQuerySourceType.InstalledForAllUsers;
            this.OnPropertyChanged(nameof(IsAllUsers));

            var mode = this._application.ApplicationState.Packages.Mode;
            this._selectedSource = this.Sources.FirstOrDefault(s => s.SourceType.Type == mode.Type && s.SourceType.Path == mode.Path) ?? this.Sources.FirstOrDefault();
            this.OnPropertyChanged(nameof(SourceType));
        }

        private void OnGetPackages(UiCancelledPayload<GetPackagesCommand> obj)
        {
            this._isAllUsers = this._application.ApplicationState.Packages.Mode.Type == PackageQuerySourceType.InstalledForAllUsers;
            this.OnPropertyChanged(nameof(IsAllUsers));

            var mode = this._application.ApplicationState.Packages.Mode;
            this._selectedSource = this.Sources.FirstOrDefault(s => s.SourceType.Type == mode.Type && s.SourceType.Path == mode.Path) ?? this.Sources.FirstOrDefault();
            this.OnPropertyChanged(nameof(SourceType));
        }

        private async void LoadContext(PackageQuerySource mode)
        {
            var executor = this._application.CommandExecutor
                .WithBusyManager(this._busyManager, OperationType.PackageLoading)
                .WithErrorHandling(this._interactionService, true);

            await executor.Invoke<GetPackagesCommand, IList<PackageEntry>>(this, new GetPackagesCommand(mode), CancellationToken.None).ConfigureAwait(false);
        }

        private void OnSetPackageFilterCommand(UiExecutedPayload<SetPackageFilterCommand> obj)
        {
            this.OnPropertyChanged(nameof(SearchKey));
        }
    }
}
