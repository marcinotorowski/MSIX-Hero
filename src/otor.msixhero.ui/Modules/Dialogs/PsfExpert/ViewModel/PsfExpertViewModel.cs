using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Otor.MsixHero.Appx.Psf.Entities;
using Otor.MsixHero.Infrastructure.Progress;
using Otor.MsixHero.Infrastructure.Services;
using Otor.MsixHero.Ui.Controls.ChangeableDialog.ViewModel;
using Otor.MsixHero.Ui.Modules.Common.PsfContent.ViewModel;
using Prism.Services.Dialogs;

namespace Otor.MsixHero.Ui.Modules.Dialogs.PsfExpert.ViewModel
{
    public class PsfExpertViewModel : ChangeableDialogViewModel, IDialogAware
    {
        public PsfExpertViewModel(IInteractionService interactionService, IDialogService dialogService) : base("MSIX Hero - PSF Expert", interactionService)
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
