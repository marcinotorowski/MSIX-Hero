using System;
using System.Threading;
using System.Threading.Tasks;
using otor.msixhero.lib.BusinessLayer.State;
using otor.msixhero.lib.Infrastructure;
using otor.msixhero.lib.Infrastructure.Configuration;
using otor.msixhero.lib.Infrastructure.Progress;
using otor.msixhero.ui.Controls.ChangeableDialog.ViewModel;
using Prism.Services.Dialogs;

namespace otor.msixhero.ui.Modules.Dialogs.PsfExpert.ViewModel
{
    public class PsfExpertViewModel : ChangeableDialogViewModel, IDialogAware
    {
        public PsfExpertViewModel(IInteractionService interactionService, IApplicationStateManager stateManager, IConfigurationService configurationService) : base("MSIX Hero - PSF Expert", interactionService)
        {
        }

        public void OnDialogOpened(IDialogParameters parameters)
        {   
        }

        protected override Task<bool> Save(CancellationToken cancellationToken, IProgress<ProgressData> progress)
        {
            return Task.FromResult(true);
        }
    }
}
