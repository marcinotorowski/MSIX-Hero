using System.Windows.Input;
using otor.msixhero.lib.Infrastructure;
using otor.msixhero.ui.Commands.RoutedCommand;
using otor.msixhero.ui.Modules.Dialogs;
using otor.msixhero.ui.Modules.Settings;
using Prism.Services.Dialogs;

namespace otor.msixhero.ui.Modules.Main.ViewModel
{
    public class BackstageCommandHandler
    {
        private readonly IDialogService dialogService;
        private readonly IInteractionService interactionService;

        public BackstageCommandHandler(IDialogService dialogService, IInteractionService interactionService)
        {
            this.dialogService = dialogService;
            this.interactionService = interactionService;

            this.Pack = new DelegateCommand(param => this.PackExecute());
            this.AppAttach = new DelegateCommand(param => this.AppAttachExecute());
            this.AppInstaller = new DelegateCommand(param => this.AppInstallerExecute(param is bool boolParam && boolParam));
            this.ModificationPackage = new DelegateCommand(param => this.ModificationPackageExecute());
            this.Settings = new DelegateCommand(param => this.SettingsExecute(param as string));
        }

        public ICommand Settings { get; }

        public ICommand AppInstaller { get; }

        public ICommand AppAttach { get; }

        public ICommand ModificationPackage { get; }

        public ICommand Pack { get; }

        private void PackExecute()
        {
            this.dialogService.ShowDialog(DialogsModule.PackPath, new DialogParameters(), this.OnDialogClosed);
        }

        private void SettingsExecute(string tab)
        {
            this.dialogService.ShowDialog(SettingsModule.Path, new DialogParameters { { "tab", tab } }, this.OnDialogClosed);
        }

        private void AppInstallerExecute(bool browse)
        {
            if (browse)
            {
                if (this.interactionService.SelectFile("Appinstaller files|*.appinstaller|All files|*.*", out var selected))
                {
                    var parameters = new DialogParameters
                    {
                        { "file", selected }
                    };

                    this.dialogService.ShowDialog(DialogsModule.AppInstallerPath, parameters, this.OnDialogClosed);
                }
            }
            else
            {
                this.dialogService.ShowDialog(DialogsModule.AppInstallerPath, new DialogParameters(), this.OnDialogClosed);
            }
        }

        private void ModificationPackageExecute()
        {
            this.dialogService.ShowDialog(DialogsModule.ModificationPackagePath, new DialogParameters(), this.OnDialogClosed);
        }

        private void AppAttachExecute()
        {
            this.dialogService.ShowDialog(DialogsModule.AppAttachPath, new DialogParameters(), this.OnDialogClosed);
        }

        private void OnDialogClosed(IDialogResult obj)
        {
        }
    }
}
