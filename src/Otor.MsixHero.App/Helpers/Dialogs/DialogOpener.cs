using System;
using System.IO;
using Otor.MsixHero.App.Hero;
using Otor.MsixHero.App.Modules;
using Otor.MsixHero.Appx.Packaging;
using Otor.MsixHero.Infrastructure.Services;
using Prism.Dialogs;
using Prism.Modularity;

namespace Otor.MsixHero.App.Helpers.Dialogs
{
    public class DialogOpener
    {
        private readonly IModuleManager _moduleManager;
        private readonly IDialogService _dialogService;
        private readonly IInteractionService _interactionService;

        public DialogOpener(IModuleManager moduleManager, IDialogService dialogService, IInteractionService interactionService)
        {
            this._moduleManager = moduleManager;
            this._dialogService = dialogService;
            this._interactionService = interactionService;
        }

        public DialogOpener(PrismServices prismServices, IInteractionService interactionService)
        {
            this._moduleManager = prismServices.ModuleManager;
            this._dialogService = prismServices.DialogService;
            this._interactionService = interactionService;
        }

        public void ShowFileDialog(string filter)
        {
            if (!_interactionService.SelectFile(FileDialogSettings.FromFilterString(filter), out var selectedFile))
            {
                return;
            }

            OpenFile(new FileInfo(selectedFile));
        }

        public void OpenFile(FileInfo file)
        {
            if (!file.Exists)
            {
                _interactionService.ShowError(string.Format(Resources.Localization.Dialogs_Error_FileNotFound_Format, file.FullName));
                return;
            }

            if (file.Name.Equals(FileConstants.AppxManifestFile, StringComparison.OrdinalIgnoreCase))
            {
                OpenMsix(file);
                return;
            }

            switch (file.Extension.ToLowerInvariant())
            {
                case FileConstants.MsixExtension:
                case FileConstants.AppxExtension:
                    OpenMsix(file);
                    break;
                case FileConstants.AppInstallerExtension:
                    OpenAppInstaller(file);
                    break;
                case FileConstants.WingetExtension:
                    OpenYaml(file);
                    break;
                default:
                    _interactionService.ShowError(string.Format(Resources.Localization.Dialogs_Error_ExtensionNotSupported_Format, file.Extension));
                    break;
            }
        }

        public void OpenMsix(FileInfo msixFile)
        {
            _moduleManager.LoadModule(ModuleNames.Dialogs.Packaging);
            var parameters = new DialogParameters
            {
                {
                    "package",
                    msixFile.FullName
                }
            };

            _dialogService.ShowDialog(NavigationPaths.DialogPaths.PackageExpert, parameters, _ => { });
        }

        // ReSharper disable once IdentifierTypo
        public void OpenYaml(FileInfo wingetFile)
        {
            _moduleManager.LoadModule(ModuleNames.Dialogs.Winget);
            var parameters = new DialogParameters
            {
                { "yaml", wingetFile.FullName}
            };

            _dialogService.ShowDialog(NavigationPaths.DialogPaths.WingetYamlEditor, parameters, _ => { });
        }

        public void OpenAppInstaller(FileInfo appInstallerFile)
        {
            _moduleManager.LoadModule(ModuleNames.Dialogs.AppInstaller);
            var parameters = new DialogParameters
            {
                { "file", appInstallerFile.FullName}
            };

            _dialogService.ShowDialog(NavigationPaths.DialogPaths.AppInstallerEditor, parameters, _ => { });
        }
    }
}
