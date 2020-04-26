using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using otor.msixhero.lib.BusinessLayer.State;
using otor.msixhero.lib.Domain.Events;
using otor.msixhero.lib.Domain.Events.PackageList;
using otor.msixhero.lib.Domain.State;
using otor.msixhero.ui.Modules.Dialogs;
using otor.msixhero.ui.Modules.Dialogs.PackageExpert.ViewModel;
using otor.msixhero.ui.Modules.Main.ViewModel;
using otor.msixhero.ui.Modules.PackageList;
using otor.msixhero.ui.Modules.PackageList.View;
using otor.msixhero.ui.Modules.VolumeManager.View;
using Prism.Events;
using Prism.Regions;
using Prism.Services.Dialogs;

namespace otor.msixhero.ui.Modules.Main.View
{
    /// <summary>
    /// Interaction logic for MainView.xaml
    /// </summary>
    public partial class MainView
    {
        private readonly IDialogService dialogService;
        private readonly IApplicationStateManager appStateManager;

        public MainView()
        {
            this.InitializeComponent();
            FocusManager.SetFocusedElement(this, this.TabControl);
            Keyboard.Focus(this.TabControl);
            this.TabControl.Focus();
        }

        public MainView(IRegionManager regionManager, IDialogService dialogService, IApplicationStateManager appStateManager) : this()
        {
            this.dialogService = dialogService;
            this.appStateManager = appStateManager;
            appStateManager.EventAggregator.GetEvent<PackagesSelectionChanged>().Subscribe(this.OnPackagesSelectionChanged, ThreadOption.UIThread);
            appStateManager.EventAggregator.GetEvent<ApplicationModeChangedEvent>().Subscribe(this.OnModeChanged, ThreadOption.UIThread);

            this.ChangeTabVisibility();
            FocusManager.SetFocusedElement(this, this.TabControl);
            Keyboard.Focus(this.TabControl);
        }

        private void ChangeTabVisibility()
        {
            var mode = this.appStateManager.CurrentState.Mode;
            switch (mode)
            {
                case ApplicationMode.Packages:
                    this.RibbonTabHome.Visibility = Visibility.Visible;
                    this.RibbonTabEdit.Visibility = Visibility.Visible;
                    this.RibbonTabCertificates.Visibility = Visibility.Visible;
                    this.RibbonTabManagement.Visibility = Visibility.Visible;
                    this.RibbonTabDeveloper.Visibility = Visibility.Visible;
                    this.RibbonTabView.Visibility = Visibility.Visible;

                    this.RibbonTabVolumesHome.Visibility = Visibility.Collapsed;
                    this.RibbonTabVolumesManagement.Visibility = Visibility.Collapsed;

                    this.RibbonTabSystemHome.Visibility = Visibility.Collapsed;

                    this.SelectedPackage.Visibility = this.appStateManager.CurrentState.Packages.SelectedItems.Any() ? Visibility.Visible : Visibility.Collapsed;
                    this.SelectedVolume.Visibility = Visibility.Collapsed;

                    this.RibbonTabSystemHome.IsSelected = true;
                    // this.Ribbon.ContextualGroups[0]
                    break;
                case ApplicationMode.VolumeManager:
                    this.RibbonTabHome.Visibility = Visibility.Collapsed;
                    this.RibbonTabEdit.Visibility = Visibility.Collapsed;
                    this.RibbonTabCertificates.Visibility = Visibility.Collapsed;
                    this.RibbonTabManagement.Visibility = Visibility.Collapsed;
                    this.RibbonTabDeveloper.Visibility = Visibility.Collapsed;
                    this.RibbonTabView.Visibility = Visibility.Collapsed;

                    this.RibbonTabVolumesHome.Visibility = Visibility.Visible;
                    this.RibbonTabVolumesManagement.Visibility = Visibility.Visible;

                    this.RibbonTabSystemHome.Visibility = Visibility.Collapsed;

                    this.SelectedPackage.Visibility = Visibility.Collapsed;
                    this.SelectedVolume.Visibility = Visibility.Visible;

                    this.RibbonTabVolumesHome.IsSelected = true;
                    break;
                case ApplicationMode.SystemStatus:
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

        private void OnPackagesSelectionChanged(PackagesSelectionChangedPayLoad obj)
        {
            this.ChangeTabVisibility();
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
