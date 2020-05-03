using System.Linq;
using System.Windows;
using System.Windows.Controls.Ribbon;
using System.Windows.Input;
using System.Windows.Threading;
using Fluent;
using otor.msixhero.lib.BusinessLayer.State;
using otor.msixhero.lib.Domain.Events;
using otor.msixhero.lib.Domain.Events.PackageList;
using otor.msixhero.lib.Domain.Events.Volumes;
using otor.msixhero.lib.Domain.State;
using otor.msixhero.lib.Infrastructure.Configuration;
using otor.msixhero.ui.Modules.Dialogs;
using Prism.Events;
using Prism.Services.Dialogs;

namespace otor.msixhero.ui.Modules.Main.View
{
    /// <summary>
    /// Interaction logic for MainView.xaml
    /// </summary>
    public partial class MainView
    {
        private readonly IConfigurationService configurationService;
        private readonly IDialogService dialogService;
        private readonly IApplicationStateManager appStateManager;

        public MainView()
        {
            this.InitializeComponent();
            FocusManager.SetFocusedElement(this, this.TabControl);
            Keyboard.Focus(this.TabControl);
            this.TabControl.Focus();
        }

        public MainView(IDialogService dialogService, IApplicationStateManager appStateManager, IConfigurationService configurationService) : this()
        {
            this.dialogService = dialogService;
            this.appStateManager = appStateManager;
            this.configurationService = configurationService;
            appStateManager.EventAggregator.GetEvent<PackagesSelectionChanged>().Subscribe(this.OnPackagesSelectionChanged, ThreadOption.UIThread);
            appStateManager.EventAggregator.GetEvent<VolumesSelectionChanged>().Subscribe(this.OnVolumesSelectionChanged, ThreadOption.UIThread);
            appStateManager.EventAggregator.GetEvent<ApplicationModeChangedEvent>().Subscribe(this.OnModeChanged, ThreadOption.UIThread);

            this.ChangeTabVisibility();

            this.Loaded += OnLoaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            this.Loaded -= this.OnLoaded;

            Application.Current.Dispatcher.Invoke(() =>
                {
                    this.MoveFocus(new TraversalRequest(FocusNavigationDirection.First));
                },
                DispatcherPriority.ApplicationIdle);
        }

        private void ChangeTabVisibility()
        {
            ApplicationMode currentHomeTab;

            if (this.RibbonTabHome.IsSelected && this.RibbonTabHome.IsVisible)
            {
                currentHomeTab = ApplicationMode.Packages;
            }
            else if (this.RibbonTabVolumesHome.IsSelected && this.RibbonTabVolumesHome.IsVisible)
            {
                currentHomeTab = ApplicationMode.VolumeManager;
            }
            else if (this.RibbonTabSystemHome.IsSelected && this.RibbonTabSystemHome.IsVisible)
            {
                currentHomeTab = ApplicationMode.SystemStatus;
            }
            else
            {
                currentHomeTab = this.appStateManager.CurrentState.Mode;
            }

            var mode = this.appStateManager.CurrentState.Mode;
            switch (mode)
            {
                case ApplicationMode.Packages:
                {
                    this.RibbonTabHome.Visibility = Visibility.Visible;
                    this.RibbonTabEdit.Visibility = Visibility.Visible;
                    this.RibbonTabCertificates.Visibility = Visibility.Visible;
                    this.RibbonTabManagement.Visibility = Visibility.Visible;
                    this.RibbonTabDeveloper.Visibility = Visibility.Visible;
                    this.RibbonTabView.Visibility = Visibility.Visible;

                    this.RibbonTabVolumesHome.Visibility = Visibility.Collapsed;
                    this.RibbonTabVolumesManagement.Visibility = Visibility.Collapsed;

                    this.RibbonTabSystemHome.Visibility = Visibility.Collapsed;

                    var wasVisible = this.SelectedPackage.IsVisible && this.Ribbon.SelectedTabItem?.IsContextual == true;
                    this.SelectedPackage.Visibility = this.appStateManager.CurrentState.Packages.SelectedItems.Any() ? Visibility.Visible : Visibility.Collapsed;
                    this.SelectedVolume.Visibility = Visibility.Collapsed;

                    if (currentHomeTab != mode || (wasVisible && !this.SelectedPackage.IsVisible))
                    {
                        this.RibbonTabHome.IsSelected = true;
                    }

                    break;
                }

                case ApplicationMode.VolumeManager:
                {
                    this.RibbonTabHome.Visibility = Visibility.Collapsed;
                    this.RibbonTabEdit.Visibility = Visibility.Collapsed;
                    this.RibbonTabCertificates.Visibility = Visibility.Collapsed;
                    this.RibbonTabManagement.Visibility = Visibility.Collapsed;
                    this.RibbonTabDeveloper.Visibility = Visibility.Collapsed;
                    this.RibbonTabView.Visibility = Visibility.Collapsed;

                    this.RibbonTabVolumesHome.Visibility = Visibility.Visible;
                    this.RibbonTabVolumesManagement.Visibility = Visibility.Visible;

                    this.RibbonTabSystemHome.Visibility = Visibility.Collapsed;

                    var wasVisible = this.SelectedVolume.IsVisible && this.Ribbon.SelectedTabItem?.IsContextual == true;
                        this.SelectedVolume.Visibility = this.appStateManager.CurrentState.Volumes.SelectedItems.Any() ? Visibility.Visible : Visibility.Collapsed;
                    this.SelectedPackage.Visibility = Visibility.Collapsed;

                    if (currentHomeTab != mode || (wasVisible && !this.SelectedVolume.IsVisible))
                    {
                        this.RibbonTabVolumesHome.IsSelected = true;
                    }

                    break;
                }

                case ApplicationMode.SystemStatus:
                {
                    this.RibbonTabHome.Visibility = Visibility.Collapsed;
                    this.RibbonTabEdit.Visibility = Visibility.Collapsed;
                    this.RibbonTabCertificates.Visibility = Visibility.Collapsed;
                    this.RibbonTabManagement.Visibility = Visibility.Collapsed;
                    this.RibbonTabDeveloper.Visibility = Visibility.Collapsed;
                    this.RibbonTabView.Visibility = Visibility.Collapsed;

                    this.RibbonTabVolumesHome.Visibility = Visibility.Collapsed;
                    this.RibbonTabVolumesManagement.Visibility = Visibility.Collapsed;

                    this.RibbonTabSystemHome.Visibility = Visibility.Visible;

                    this.SelectedPackage.Visibility = Visibility.Collapsed;
                    this.SelectedVolume.Visibility = Visibility.Collapsed;

                    this.RibbonTabSystemHome.IsSelected = true;
                    break;
                }
            }
        }

        private void OnPackagesSelectionChanged(PackagesSelectionChangedPayLoad obj)
        {
            var wasVisible = this.SelectedPackage.Items.Any(item => item.IsSelected);
            this.ChangeTabVisibility();

            if (!obj.IsExplicit)
            {
                // The selection was not invoked by the user, so do not change the tab selection
                return;
            }

            if (!wasVisible && this.SelectedPackage.IsVisible)
            {
                var config = this.configurationService.GetCurrentConfiguration();
                if (config.UiConfiguration?.SwitchToContextTabAfterSelection == true)
                {
                    this.SelectedPackage.FirstVisibleItem.IsSelected = true;
                }
            }
            else if (wasVisible && !this.SelectedPackage.IsVisible)
            {
                this.RibbonTabHome.IsSelected = true;
            }
        }

        private void OnVolumesSelectionChanged(VolumesSelectionChangedPayLoad obj)
        {
            var wasVisible = this.SelectedVolume.Items.Any(item => item.IsSelected);
            this.ChangeTabVisibility();

            if (!wasVisible && this.SelectedVolume.IsVisible)
            {
                var config = this.configurationService.GetCurrentConfiguration();
                if (config.UiConfiguration?.SwitchToContextTabAfterSelection == true)
                {
                    this.SelectedVolume.FirstVisibleItem.IsSelected = true;
                }
            }
            else if (wasVisible && !this.SelectedVolume.IsVisible)
            {
                this.RibbonTabVolumesHome.IsSelected = true;
            }
        }

        private void OnModeChanged(ApplicationMode mode)
        {
            this.ChangeTabVisibility();
            switch (mode)
            {
                case ApplicationMode.VolumeManager:
                    this.RibbonTabVolumesHome.IsSelected = true;
                    break;
                case ApplicationMode.Packages:
                    this.RibbonTabHome.IsSelected = true;
                    break;
            }
        }

        private void Close_OnExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            // ReSharper disable once PossibleNullReferenceException
            Application.Current.MainWindow.Close();
        }

        private void OnDrop(object sender, DragEventArgs e)
        {
            string[] droppedFiles = null;
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                droppedFiles = e.Data.GetData(DataFormats.FileDrop, true) as string[];
            }

            if ((null == droppedFiles) || !droppedFiles.Any())
            {
                return;
            }

            var handler = new ExplorerHandler(this.dialogService);
            handler.Handle(droppedFiles);
        }
    }
}
