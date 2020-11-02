using System.Windows.Input;
using Otor.MsixHero.Ui.Commands.RoutedCommand;
using Otor.MsixHero.Ui.Modules.Common.PsfContent.ViewModel.Items.Redirection;
using Prism.Services.Dialogs;

namespace Otor.MsixHero.Ui.Modules.Dialogs.PsfExpert.ViewModel
{
    public class PsfExpertCommandHandler
    {
        private readonly IDialogService dialogService;

        public PsfExpertCommandHandler(IDialogService dialogService)
        {
            this.dialogService = dialogService;
            this.Edit = new DelegateCommand(this.EditExecute, this.CanEditExecute);
        }


        public ICommand Edit { get; }

        private void EditExecute(object target)
        {
            if (target is PsfContentFileRedirectionViewModel redirectionFile)
            {
                this.EditExecute(redirectionFile);
            }
        }

        private void EditExecute(PsfContentFileRedirectionViewModel rule)
        {
            var param = new DialogParameters();
            param.Add("value", rule.RegularExpression);

            this.dialogService.ShowDialog(Constants.PathPackageExpertFileRule, param, this.OnDialogClosed);
        }

        private bool CanEditExecute(object target)
        {
            if (target is PsfContentFileRedirectionViewModel redirectionFile)
            {
                return this.CanEditExecute(redirectionFile);
            }

            return false;
        }

        private bool CanEditExecute(PsfContentFileRedirectionViewModel target)
        {
            return true;
        }

        private void OnDialogClosed(IDialogResult obj)
        {
        }
    }
}
