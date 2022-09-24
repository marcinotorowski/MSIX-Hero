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

using Otor.MsixHero.App.Hero;
using Otor.MsixHero.App.Hero.Commands.Containers;
using Otor.MsixHero.App.Hero.Events.Base;
using Otor.MsixHero.App.Mvvm;
using Otor.MsixHero.Infrastructure.Configuration;

namespace Otor.MsixHero.App.Modules.Containers.Search.ViewModels
{
    public class ContainersFilterSortViewModel : NotifyPropertyChanged
    {
        private readonly IMsixHeroApplication _application;

        public ContainersFilterSortViewModel(IMsixHeroApplication application)
        {
            this._application = application;            
            this._application.EventAggregator.GetEvent<UiExecutedEvent<SetSharedPackageContainersSortingCommand>>().Subscribe(this.OnSetSorting);
        }
        
        public bool IsDescending
        {
            get => this._application.ApplicationState.Containers.SortDescending;
            set => this._application.CommandExecutor.Invoke(this, new SetSharedPackageContainersSortingCommand(this.Sort, value));
        }

        public ContainerSort Sort
        {
            get => this._application.ApplicationState.Containers.SortMode;
            set => this._application.CommandExecutor.Invoke(this, new SetSharedPackageContainersSortingCommand(value, this.IsDescending));
        }
        private void OnSetSorting(UiExecutedPayload<SetSharedPackageContainersSortingCommand> obj)
        {
            this.OnPropertyChanged(nameof(IsDescending));
            this.OnPropertyChanged(nameof(Sort));
        }
    }
}
