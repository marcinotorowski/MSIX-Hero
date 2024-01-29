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

using Otor.MsixHero.App.Hero;
using Otor.MsixHero.App.Hero.Commands.Tools;
using Otor.MsixHero.App.Hero.Events.Base;
using Otor.MsixHero.App.Mvvm;
using Prism.Events;

namespace Otor.MsixHero.App.Modules.Tools.ViewModels
{
    public class ToolsSearchViewModel : NotifyPropertyChanged
    {
        private readonly IMsixHeroApplication _application;

        public ToolsSearchViewModel(IMsixHeroApplication application, IEventAggregator eventAggregator)
        {
            this._application = application;
            eventAggregator.GetEvent<UiExecutedEvent<SetToolFilterCommand>>().Subscribe(this.OnSetToolFilterCommand);
        }

        public string SearchKey
        {
            get => this._application.ApplicationState.Tools.SearchKey;

            set
            {
                if (this._application.ApplicationState.Tools.SearchKey == value)
                {
                    return;
                }

                this._application.CommandExecutor.Invoke(this, new SetToolFilterCommand(value));
            }
        }

        private void OnSetToolFilterCommand(UiExecutedPayload<SetToolFilterCommand> obj)
        {
            this.OnPropertyChanged(nameof(this.SearchKey));
        }
    }
}
