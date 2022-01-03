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

using System.Diagnostics;
using System.Windows.Input;
using Prism.Commands;

namespace Otor.MsixHero.App.Modules.SystemStatus.ViewModel
{
    public class SystemStatusCommandHandler
    {
        public SystemStatusCommandHandler()
        {
            this.OpenAppsFeatures = new DelegateCommand(this.OpenAppsFeaturesExecute);
            this.OpenDevSettings = new DelegateCommand(this.OpenDevSettingsExecute);
            this.OpenServices = new DelegateCommand(this.OpenServicesExecute);
        }

        public ICommand OpenAppsFeatures { get; }

        public ICommand OpenDevSettings { get; }

        public ICommand OpenServices { get; }

        private void OpenAppsFeaturesExecute()
        {
            var process = new ProcessStartInfo("ms-settings:appsfeatures") { UseShellExecute = true };
            Process.Start(process);
        }

        private void OpenServicesExecute()
        {
            var process = new ProcessStartInfo("services.msc") { UseShellExecute = true };
            Process.Start(process);
        }

        private void OpenDevSettingsExecute()
        {
            var process = new ProcessStartInfo("ms-settings:developers") { UseShellExecute = true };
            Process.Start(process);
        }
    }
}
