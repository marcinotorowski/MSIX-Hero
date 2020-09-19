using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using Fluent;
using Otor.MsixHero.Infrastructure.Services;
using Otor.MsixHero.Lib.Infrastructure.Progress;
using Otor.MsixHero.Ui.Hero;
using Otor.MsixHero.Ui.Hero.Commands;
using Otor.MsixHero.Ui.Hero.Commands.Packages;
using Otor.MsixHero.Ui.Hero.Commands.Volumes;
using Otor.MsixHero.Ui.Hero.Events.Base;
using Otor.MsixHero.Ui.Hero.State;
using Otor.MsixHero.Ui.Modules.Dialogs;
using Prism.Events;
using Prism.Services.Dialogs;

namespace Otor.MsixHero.Ui.Modules.Main.View
{
    /// <summary>
    /// Interaction logic for MainView.xaml
    /// </summary>
    public partial class MainView
    {
        private readonly IConfigurationService configurationService;
        private readonly IDialogService dialogService;
        private readonly IMsixHeroApplication application;
        private readonly IBusyManager busyManager;

        public MainView()
        {
            this.InitializeComponent();
            FocusManager.SetFocusedElement(this, this.TabControl);
            Keyboard.Focus(this.TabControl);
            this.TabControl.Focus();
        }

        public MainView(
            IDialogService dialogService, 
            IMsixHeroApplication application,
            IBusyManager busyManager,
            IConfigurationService configurationService) : this()
        {
            this.dialogService = dialogService;
            this.application = application;
            this.busyManager = busyManager;
            this.configurationService = configurationService;

            application.EventAggregator.GetEvent<UiExecutedEvent<SelectPackagesCommand>>().Subscribe(this.OnSelectPackages, ThreadOption.UIThread);
            application.EventAggregator.GetEvent<UiExecutedEvent<SelectVolumesCommand>>().Subscribe(this.OnSelectVolumes, ThreadOption.UIThread);
            application.EventAggregator.GetEvent<UiExecutedEvent<GetPackagesCommand>>().Subscribe(this.OnGetPackages, ThreadOption.UIThread);
            application.EventAggregator.GetEvent<UiExecutedEvent<GetVolumesCommand>>().Subscribe(this.OnGetVolumes, ThreadOption.UIThread);
            this.application.EventAggregator.GetEvent<UiExecutedEvent<SetCurrentModeCommand>>().Subscribe(this.OnSetCurrentMode, ThreadOption.UIThread);

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
            else if (this.RibbonTabEventViewerHome.IsSelected && this.RibbonTabEventViewerHome.IsVisible)
            {
                currentHomeTab = ApplicationMode.EventViewer;
            }
            else
            {
                currentHomeTab = this.application.ApplicationState.CurrentMode;
            }

            var mode = this.application.ApplicationState.CurrentMode;
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
                    this.SelectedPackage.Visibility = this.application.ApplicationState.Packages.SelectedPackages.Any() ? Visibility.Visible : Visibility.Collapsed;
                    this.SelectedVolume.Visibility = Visibility.Collapsed;

                    this.RibbonTabEventViewerHome.Visibility = Visibility.Collapsed;

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
                    this.SelectedVolume.Visibility = this.application.ApplicationState.Volumes.SelectedVolumes.Any() ? Visibility.Visible : Visibility.Collapsed;
                    this.SelectedPackage.Visibility = Visibility.Collapsed;

                    this.RibbonTabEventViewerHome.Visibility = Visibility.Collapsed;

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

                    this.RibbonTabEventViewerHome.Visibility = Visibility.Collapsed;

                        this.RibbonTabSystemHome.IsSelected = true;
                    break;
                }

                case ApplicationMode.EventViewer:
                {
                    this.RibbonTabHome.Visibility = Visibility.Collapsed;
                    this.RibbonTabEdit.Visibility = Visibility.Collapsed;
                    this.RibbonTabCertificates.Visibility = Visibility.Collapsed;
                    this.RibbonTabManagement.Visibility = Visibility.Collapsed;
                    this.RibbonTabDeveloper.Visibility = Visibility.Collapsed;
                    this.RibbonTabView.Visibility = Visibility.Collapsed;

                    this.RibbonTabVolumesHome.Visibility = Visibility.Collapsed;
                    this.RibbonTabVolumesManagement.Visibility = Visibility.Collapsed;

                    this.RibbonTabSystemHome.Visibility = Visibility.Collapsed;

                    this.SelectedPackage.Visibility = Visibility.Collapsed;
                    this.SelectedVolume.Visibility = Visibility.Collapsed;

                    this.RibbonTabEventViewerHome.Visibility = Visibility.Visible;
                    this.RibbonTabEventViewerHome.IsSelected = true;
                    break;
                }
            }
        }

        private void OnGetPackages(UiExecutedPayload<GetPackagesCommand> obj)
        {
            this.HandlePackageSelectionChange(false);
        }

        private void OnGetVolumes(UiExecutedPayload<GetVolumesCommand> obj)
        {
            this.HandleVolumeSelectionChange(false);
        }

        private void HandleVolumeSelectionChange(bool allowSelectionChange = true)
        {
            var wasVisible = this.SelectedVolume.Items.Any(item => item.IsSelected);
            this.ChangeTabVisibility();

            this.SelectedVolume.Visibility = this.application.ApplicationState.Volumes.SelectedVolumes.Count > 0 ? Visibility.Visible : Visibility.Collapsed;

            if (!wasVisible && this.SelectedVolume.IsVisible)
            {
                var config = this.configurationService.GetCurrentConfiguration();
                if (config.UiConfiguration?.SwitchToContextTabAfterSelection == true && allowSelectionChange)
                {
                    this.SelectedVolume.FirstVisibleItem.IsSelected = true;
                }
            }
            else if (wasVisible && !this.SelectedVolume.IsVisible)
            {
                this.RibbonTabVolumesHome.IsSelected = true;
            }
        }

        private void HandlePackageSelectionChange(bool allowSelectionChange = true)
        {
            var wasVisible = this.SelectedPackage.Items.Any(item => item.IsSelected);
            this.ChangeTabVisibility();

            this.SelectedPackage.Visibility = this.application.ApplicationState.Packages.SelectedPackages.Count > 0 ? Visibility.Visible : Visibility.Collapsed;

            if (!wasVisible && this.SelectedPackage.IsVisible)
            {
                var config = this.configurationService.GetCurrentConfiguration();
                if (config.UiConfiguration?.SwitchToContextTabAfterSelection == true && allowSelectionChange)
                {
                    this.SelectedPackage.FirstVisibleItem.IsSelected = true;
                }
            }
            else if (wasVisible && !this.SelectedPackage.IsVisible)
            {
                this.RibbonTabHome.IsSelected = true;
            }
        }

        private void OnSelectPackages(UiExecutedPayload<SelectPackagesCommand> obj)
        {
            this.HandlePackageSelectionChange();
        }

        private void OnSelectVolumes(UiExecutedPayload<SelectVolumesCommand> obj)
        {
            this.HandleVolumeSelectionChange();
        }

        private void OnSetCurrentMode(UiExecutedPayload<SetCurrentModeCommand> mode)
        {
            this.ChangeTabVisibility();
            switch (this.application.ApplicationState.CurrentMode)
            {
                case ApplicationMode.VolumeManager:
                    this.RibbonTabVolumesHome.IsSelected = true;
                    break;
                case ApplicationMode.Packages:
                    this.RibbonTabHome.IsSelected = true;
                    break;
                case ApplicationMode.EventViewer:
                    this.RibbonTabEventViewerHome.IsSelected = true;
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

        private void SplitButtonClicked(object sender, RoutedEventArgs e)
        {
            var findChildren = ((SplitButton) sender).Items.OfType<System.Windows.Controls.RadioButton>().Where(b => b.IsEnabled).ToArray();

            for (var i = 0; i < findChildren.Length; i++)
            {
                var isChecked = findChildren[i].IsChecked;
                if (isChecked == true)
                {
                    findChildren[(i + 1) % findChildren.Length].IsChecked = true;
                    break;
                }
            }
        }
    }
}
