using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Otor.MsixHero.Ui.Modules.Dialogs.PackageExpert.ViewModel;
using Prism.Regions;
using Prism.Services.Dialogs;

namespace Otor.MsixHero.Ui.Modules.Dialogs
{
    public class ExplorerHandler
    {
        private readonly IDialogService dialogService;
        private readonly bool openAsModal;
        private readonly IRegionManager regionManager;

        public ExplorerHandler(IDialogService dialogService, bool openAsModal = false)
        {
            this.dialogService = dialogService;
            this.openAsModal = openAsModal;
        }

        public ExplorerHandler(IRegionManager regionManager)
        {
            this.regionManager = regionManager;
        }

        public void Handle(params string[] files)
        {
            this.Handle(((IEnumerable<string>)files));
        }

        public void Handle(IEnumerable<string> files)
        {
            if (this.regionManager != null || this.openAsModal)
            {
                files = files.Take(1);
            }
            else
            {
                files = files.Take(10); // do not open more than 10 at once.
            }

            foreach (var file in files)
            {
                var extension = Path.GetExtension(file)?.ToLowerInvariant();
                switch (extension)
                {
                    case ".appinstaller":
                        if (this.regionManager != null)
                        {
                            var navigationParams = new NavigationParameters {{"file", file}};

                            this.regionManager.Regions[Constants.RegionPackageExpertShell].RequestNavigate(new Uri(Constants.PathAppInstaller, UriKind.Relative), navigationParams);
                        }
                        else
                        {
                            var parameters = new DialogParameters{{ "file", file }};

                            if (this.openAsModal)
                            {
                                this.dialogService.ShowDialog(Constants.PathAppInstaller, parameters, result => { });
                            }
                            else
                            {
                                this.dialogService.Show(Constants.PathAppInstaller, parameters, result => { });
                            }
                        }

                        break;

                    case ".yaml":

                        if (this.regionManager != null)
                        {
                            var navigationParams = new NavigationParameters { { "yaml", file } };
                            this.regionManager.Regions[Constants.RegionPackageExpertShell].RequestNavigate(new Uri(Constants.PathWinget, UriKind.Relative), navigationParams);
                        }
                        else
                        {
                            var parameters = new DialogParameters { { "yaml", file } };

                            if (this.openAsModal)
                            {
                                this.dialogService.ShowDialog(Constants.PathWinget, parameters, result => { });
                            }
                            else
                            {
                                this.dialogService.Show(Constants.PathWinget, parameters, result => { });
                            }
                        }

                        break;

                    default:

                        if (this.regionManager != null)
                        {
                            var navigationParams = new PackageExpertSelection(file).ToNavigationParameters();

                            this.regionManager.Regions[Constants.RegionPackageExpertShell].RequestNavigate(new Uri(Constants.PathPackageExpert, UriKind.Relative), navigationParams);
                        }
                        else
                        {
                            var parameters = new PackageExpertSelection(file).ToDialogParameters();

                            if (this.openAsModal)
                            {
                                this.dialogService.ShowDialog(Constants.PathPackageExpert, parameters, result => { });
                            }
                            else
                            {
                                this.dialogService.Show(Constants.PathPackageExpert, parameters, result => { });
                            }
                        }

                        break;
                }
            }
        }
    }
}
