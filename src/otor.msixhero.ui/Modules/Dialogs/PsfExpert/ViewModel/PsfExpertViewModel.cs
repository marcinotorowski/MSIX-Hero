using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using otor.msixhero.lib.BusinessLayer.State;
using otor.msixhero.lib.Domain.Appx.Psf;
using otor.msixhero.lib.Infrastructure;
using otor.msixhero.lib.Infrastructure.Configuration;
using otor.msixhero.lib.Infrastructure.Progress;
using otor.msixhero.ui.Controls.ChangeableDialog.ViewModel;
using otor.msixhero.ui.Modules.Common.PsfContent.ViewModel;
using Prism.Services.Dialogs;

namespace otor.msixhero.ui.Modules.Dialogs.PsfExpert.ViewModel
{
    public class PsfExpertViewModel : ChangeableDialogViewModel, IDialogAware
    {
        public PsfExpertViewModel(IInteractionService interactionService, IDialogService dialogService, IApplicationStateManager stateManager, IConfigurationService configurationService) : base("MSIX Hero - PSF Expert", interactionService)
        {
            this.CommandHandler = new PsfExpertCommandHandler(dialogService);
        }

        public PsfExpertCommandHandler CommandHandler { get; }

        public PsfContentViewModel Content { get; private set; }

        public void OnDialogOpened(IDialogParameters parameters)
        {
            var psfSerializer = new PsfConfigSerializer();
            var json = File.ReadAllText(@"E:\temp\msix-psf\fixed-rayeval\config.json");
            if (this.Content != null)
            {
                this.RemoveChild(this.Content);
            }

            this.Content = new PsfContentViewModel(psfSerializer.Deserialize(json));
            this.OnPropertyChanged(nameof(this.Content));
            this.AddChild(this.Content);
        }

        protected override Task<bool> Save(CancellationToken cancellationToken, IProgress<ProgressData> progress)
        {
            return Task.FromResult(true);
        }
    }
}
