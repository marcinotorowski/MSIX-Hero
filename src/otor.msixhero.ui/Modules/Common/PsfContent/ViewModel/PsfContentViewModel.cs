using System.Linq;
using otor.msixhero.lib.Domain.Appx.Psf;
using otor.msixhero.ui.Domain;
using otor.msixhero.ui.Modules.Common.PsfContent.ViewModel.Items;

namespace otor.msixhero.ui.Modules.Common.PsfContent.ViewModel
{
    public class PsfContentViewModel : ChangeableContainer
    {
        public PsfContentViewModel(PsfConfig psfConfig)
        {
            this.RedirectionRules = new ChangeableCollection<PsfContentProcessViewModel>();

            this.Setup(psfConfig);

            this.RedirectionRules.Commit();
            this.AddChild(this.RedirectionRules);
        }

        public ChangeableCollection<PsfContentProcessViewModel> RedirectionRules { get; }

        private void Setup(PsfConfig psfConfig)
        {
            if (psfConfig?.Processes == null)
            {
                return;
            }

            foreach (var process in psfConfig.Processes)
            {
                foreach (var item in process.Fixups.Where(f => f.Config != null && f.Config is PsfRedirectionFixupConfig))
                {
                    var config = (PsfRedirectionFixupConfig) item.Config;
                    if (config == null)
                    {
                        continue;
                    }

                    var psfContentProcessViewModel = new PsfContentProcessViewModel(process.Executable, item.Dll, config.RedirectedPaths);
                    this.RedirectionRules.Add(psfContentProcessViewModel);
                }
            }
        }
    }
}
