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
using System.Linq;
using System.Threading.Tasks;
using Otor.MsixHero.App.Hero.Events;
using Otor.MsixHero.App.Modules.PackageManagement.PackageContent.ViewModel.Common;
using Otor.MsixHero.App.Mvvm;
using Otor.MsixHero.Appx.Packaging.Manifest.Entities;
using Otor.MsixHero.Infrastructure.Configuration;
using Otor.MsixHero.Infrastructure.Services;
using Prism.Events;

namespace Otor.MsixHero.App.Modules.PackageManagement.PackageContent.ViewModel.Overview.Actions.Start;

public class StartViewModel : NotifyPropertyChanged, ILoadPackage
{
    private readonly IConfigurationService _configurationService;
    private ObservableCollection<ToolViewModel> _tools;

    public StartViewModel(IEventAggregator eventAggregator, IConfigurationService configurationService)
    {
        _configurationService = configurationService;
        eventAggregator.GetEvent<ToolsChangedEvent>().Subscribe(OnToolsChanged, ThreadOption.UIThread);
    }

    public ObservableCollection<ApplicationViewModel> Applications { get; private set; }

    public ObservableCollection<ToolViewModel> Tools
    {
        get
        {
            if (_tools == null)
            {
                CreateTools(false);
            }

            return _tools;
        }
    }

    public Task LoadPackage(AppxPackage model, string filePath)
    {
        var apps = new ObservableCollection<ApplicationViewModel>();

        if (model.Applications != null)
        {
            foreach (var item in model.Applications)
            {
                apps.Add(new ApplicationViewModel(item, model));
            }
        }

        Applications = apps;
        OnPropertyChanged(nameof(Applications));

        return Task.CompletedTask;
    }

    private void CreateTools(bool raisePropertyChangedEvents)
    {
        var cfg = _configurationService.GetCurrentConfiguration();
        var currentTools = cfg.Packages?.Tools;

        _tools = new ObservableCollection<ToolViewModel>();

        if (currentTools?.Any() == true)
        {
            foreach (var item in currentTools)
            {
                _tools.Add(new ToolViewModel(item));
            }
        }

        if (raisePropertyChangedEvents)
        {
            OnPropertyChanged(nameof(Tools));
        }
    }

    private void OnToolsChanged(IReadOnlyCollection<ToolListConfiguration> obj)
    {
        CreateTools(true);
    }
}