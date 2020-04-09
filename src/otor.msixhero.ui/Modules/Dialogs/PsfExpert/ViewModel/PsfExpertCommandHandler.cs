using System.Windows.Input;
using otor.msixhero.ui.Commands.RoutedCommand;
using otor.msixhero.ui.Modules.Dialogs.PsfExpert.Elements.ViewModel;
using otor.msixhero.ui.Modules.Dialogs.PsfExpert.ViewModel.Items;
using Prism.Services.Dialogs;

namespace otor.msixhero.ui.Modules.Dialogs.PsfExpert.ViewModel
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
            if (target is PsfExpertFilePatternViewModel)
            {
                this.EditExecute(((PsfExpertFilePatternViewModel)target));
            }
        }

        private void EditExecute(PsfExpertFilePatternViewModel rule)
        {
            var param = new DialogParameters();
            param.Add("value", rule.Pattern);

            this.dialogService.ShowDialog(Constants.PathPackageExpertFileRule, param, this.OnDialogClosed);
        }

        private bool CanEditExecute(object target)
        {
            if (target is PsfExpertFilePatternViewModel)
            {
                return true;
            }

            return false;
        }

        private void OnDialogClosed(IDialogResult obj)
        {
        }
    }
}
