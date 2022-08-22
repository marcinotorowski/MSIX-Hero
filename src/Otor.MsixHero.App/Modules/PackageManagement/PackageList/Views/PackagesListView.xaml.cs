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
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Otor.MsixHero.App.Helpers.Interop;
using Otor.MsixHero.App.Hero;
using Otor.MsixHero.App.Hero.Commands;
using Otor.MsixHero.App.Hero.Events;
using Otor.MsixHero.Infrastructure.Configuration;
using Otor.MsixHero.Infrastructure.Services;

namespace Otor.MsixHero.App.Modules.PackageManagement.PackageList.Views
{
    /// <summary>
    /// Interaction logic for PackagesListView.
    /// </summary>
    public partial class PackagesListView
    {
        private readonly IConfigurationService _configService;
        private IList<MenuItem> _tools;

        public PackagesListView(IMsixHeroApplication application, IConfigurationService configService)
        {
            this._configService = configService;

            application.EventAggregator.GetEvent<ToolsChangedEvent>().Subscribe(_ => this._tools = null);
            this.InitializeComponent();
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
