using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Otor.MsixHero.App.Helpers;
using Otor.MsixHero.App.Hero;
using Otor.MsixHero.App.Hero.Commands.Packages;
using Otor.MsixHero.App.Hero.Events;
using Otor.MsixHero.App.Hero.Events.Base;
using Otor.MsixHero.App.Modules.PackageManagement.ViewModels;
using Otor.MsixHero.App.Modules.PackageManagement.ViewModels.Items;
using Otor.MsixHero.Infrastructure.Services;
using Prism.Events;

namespace Otor.MsixHero.App.Modules.PackageManagement.Views
{
    /// <summary>
    /// Interaction logic for PackagesListView.
    /// </summary>
    public partial class PackagesListView
    {
        private readonly IMsixHeroApplication application;
        private readonly IConfigurationService configService;
        private IList<MenuItem> tools;

        public PackagesListView(
            IMsixHeroApplication application, 
            IConfigurationService configService)
        {
            this.application = application;
            this.configService = configService;

            application.EventAggregator.GetEvent<ToolsChangedEvent>().Subscribe(payload => this.tools = null);
            application.EventAggregator.GetEvent<UiFailedEvent<GetPackagesCommand>>().Subscribe(this.OnGetPackagesFailed, ThreadOption.UIThread);
            application.EventAggregator.GetEvent<UiExecutingEvent<GetPackagesCommand>>().Subscribe(this.OnGetPackagesExecuting);
            application.EventAggregator.GetEvent<UiCancelledEvent<GetPackagesCommand>>().Subscribe(this.OnGetPackagesCancelled, ThreadOption.UIThread);
            application.EventAggregator.GetEvent<UiExecutedEvent<GetPackagesCommand>>().Subscribe(this.OnGetPackagesExecuted, ThreadOption.UIThread);
            
            this.InitializeComponent();
            this.ListBox.PreviewKeyDown += ListBoxOnKeyDown;
            this.ListBox.PreviewKeyUp += ListBoxOnKeyUp;
        }

        private void ListBoxOnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Down || e.Key == Key.Up || e.Key == Key.PageDown || e.Key == Key.PageUp)
            {
                this.ListBox.SelectionChanged -= this.OnSelectionChanged;
            }
        }

        private void ListBoxOnKeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Down || e.Key == Key.Up || e.Key == Key.PageDown || e.Key == Key.PageUp)
            {
                this.ListBox.SelectionChanged += this.OnSelectionChanged;

                this.application.CommandExecutor.Invoke(this, new SelectPackagesCommand(this.ListBox.SelectedItems.OfType<InstalledPackageViewModel>().Select(p => p.ManifestLocation)));
            }
        }

        private void OnGetPackagesExecuting(UiExecutingPayload<GetPackagesCommand> obj)
        {
            this.ListBox.SelectionChanged -= this.OnSelectionChanged;
        }

        private void OnGetPackagesFailed(UiFailedPayload<GetPackagesCommand> obj)
        {
            this.ListBox.SelectionChanged += this.OnSelectionChanged;
        }

        private void OnGetPackagesCancelled(UiCancelledPayload<GetPackagesCommand> obj)
        {
            this.ListBox.SelectionChanged += this.OnSelectionChanged;
        }

        private void OnGetPackagesExecuted(UiExecutedPayload<GetPackagesCommand> obj)
        {
            this.ListBox.SelectedItems.Clear();

            foreach (var item in this.ListBox.Items.OfType<InstalledPackageViewModel>())
            {
                if (!this.application.ApplicationState.Packages.SelectedPackages.Contains(item.Model))
                {
                    continue;
                }

                this.ListBox.SelectedItems.Add(item);
            }

            this.ListBox.SelectionChanged += this.OnSelectionChanged;
        }

        private void PackageContextMenu_OnContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            if (this.tools != null)
            {
                return;
            }

            this.SetTools();
            var frameworkElement = (FrameworkElement)sender;
            // ReSharper disable once PossibleNullReferenceException
            var lastMenu = frameworkElement.ContextMenu.Items.OfType<MenuItem>().Last();

            lastMenu.Items.Clear();
            foreach (var item in this.tools)
            {
                lastMenu.Items.Add(item);
            }

            lastMenu.Items.Add(new Separator());
            lastMenu.Items.Add(new MenuItem
            {
                Command = MsixHeroCommands.Settings,
                CommandParameter = "tools",
                Header = "More commands..."
            });
        }

        private void SetTools()
        {
            if (this.tools != null)
            {
                return;
            }

            this.tools = new List<MenuItem>();
            var configuredTools = this.configService.GetCurrentConfiguration().List.Tools;

            foreach (var item in configuredTools)
            {
                this.tools.Add(new MenuItem
                {
                    Command = MsixHeroCommands.RunTool,
                    Icon = new Image { Source = ShellIcon.GetIconFor(string.IsNullOrEmpty(item.Icon) ? item.Path : item.Icon) },
                    Header = item.Name,
                    CommandParameter = item
                });
            }
        }

        private void OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            this.application.CommandExecutor.Invoke(this, new SelectPackagesCommand(this.ListBox.SelectedItems.OfType<InstalledPackageViewModel>().Select(p => p.ManifestLocation)));
        }
    }
}
