using System.Windows.Input;
using otor.msixhero.lib.Infrastructure;
using otor.msixhero.lib.Infrastructure.Configuration;
using otor.msixhero.ui.Commands.RoutedCommand;
using otor.msixhero.ui.Modules.Dialogs;
using otor.msixhero.ui.Modules.Settings;
using Prism.Services.Dialogs;

namespace otor.msixhero.ui.Modules.Main.ViewModel
{
    public class MainCommandHandler
    {
        private readonly IDialogService dialogService;

        public MainCommandHandler(IDialogService dialogService)
        {
            this.dialogService = dialogService;

            this.Settings = new DelegateCommand(param => this.SettingsExecute(param as string), param => true);
            this.Help = new DelegateCommand(param => this.HelpExecute());
        }
        public ICommand Help { get; }

        public ICommand Settings { get; }
        
        private void HelpExecute()
        {
            this.dialogService.ShowDialog(DialogsModule.HelpPath, new DialogParameters(), this.OnDialogClosed);
        }

        private void SettingsExecute(string tab)
        {
            this.dialogService.ShowDialog(SettingsModule.Path, new DialogParameters { { "tab", tab } }, this.OnDialogClosed);
        }

        private void OnDialogClosed(IDialogResult obj)
        {
        }
    }
}
