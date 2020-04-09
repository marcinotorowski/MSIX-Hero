using System.Collections.Generic;
using otor.msixhero.ui.Domain;

namespace otor.msixhero.ui.Modules.Dialogs.PsfExpert.ViewModel.Items
{
    public class PsfExpertRedirectionsViewModel : ChangeableContainer
    {
        public PsfExpertRedirectionsViewModel(IEnumerable<PsfExpertRedirectionIfProcessViewModel> redirections)
        {
            this.Redirections = new ChangeableCollection<PsfExpertRedirectionIfProcessViewModel>(redirections);
            this.Redirections.Commit();
        }

        public ChangeableCollection<PsfExpertRedirectionIfProcessViewModel> Redirections { get; }
    }
}