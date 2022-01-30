using System;
using System.IO;
using Otor.MsixHero.App.Modules;
using Otor.MsixHero.Appx.Packaging;
using Otor.MsixHero.Infrastructure.Services;
using Prism.Modularity;
using Prism.Services.Dialogs;

namespace Otor.MsixHero.App.Helpers
{
    public class DialogOpener
    {
        private readonly IModuleManager moduleManager;
        private readonly IDialogService dialogService;
        private readonly IInteractionService interactionService;

        public DialogOpener(IModuleManager moduleManager, IDialogService dialogService, IInteractionService interactionService)
        {
            this.moduleManager = moduleManager;
            this.dialogService = dialogService;
            this.interactionService = interactionService;
        }
        
        public void ShowFileDialog(string filter)
        {
            if (!this.interactionService.SelectFile(FileDialogSettings.FromFilterString(filter), out var selectedFile))
            {
                return;
            }

            this.OpenFile(new FileInfo(selectedFile));
        }

        public void OpenFile(FileInfo file)
        {
            if (!file.Exists)
            {
                this.interactionService.ShowError(string.Format(Resources.Localization.Dialogs_Error_FileNotFound_Format, file.FullName));
                return;
            }

            if (file.Name.Equals(FileConstants.AppxManifestFile, StringComparison.OrdinalIgnoreCase))
            {
                this.OpenMsix(file);
                return;
            }
            
            switch (file.Extension.ToLowerInvariant())
            {
                case FileConstants.MsixExtension:
                case FileConstants.AppxExtension:
                    this.OpenMsix(file);
                    break;
                case FileConstants.AppInstallerExtension:
                    this.OpenAppInstaller(file);
                    break;
                case FileConstants.WingetExtension:
                    this.OpenYaml(file);
                    break;
                default:
                    this.interactionService.ShowError(string.Format(Resources.Localization.Dialogs_Error_ExtensionNotSupported_Format, file.Extension));
                    break;
            }
        }

        public void OpenMsix(FileInfo msixFile)
        {
            this.moduleManager.LoadModule(ModuleNames.Dialogs.Packaging);
            var parameters = new DialogParameters
            {
                {
                    "package",
                    msixFile.FullName
                }
            };
            
            this.dialogService.ShowDialog(NavigationPaths.DialogPaths.PackageExpert, parameters, _ => { });
        }

        // ReSharper disable once IdentifierTypo
        public void OpenYaml(FileInfo wingetFile)
        {
            this.moduleManager.LoadModule(ModuleNames.Dialogs.Winget);
            var parameters = new DialogParameters
            {
                {"yaml", wingetFile.FullName}
            };

            this.dialogService.ShowDialog(NavigationPaths.DialogPaths.WingetYamlEditor, parameters, _ => { });
        }

        public void OpenAppInstaller(FileInfo appInstallerFile)
        {
            this.moduleManager.LoadModule(ModuleNames.Dialogs.AppInstaller);
            var parameters = new DialogParameters
            {
                {"file", appInstallerFile.FullName}
            };

            this.dialogService.ShowDialog(NavigationPaths.DialogPaths.AppInstallerEditor, parameters, _ => { });
        }
    }
}
