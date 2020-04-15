using System.Linq;
using otor.msixhero.lib.Domain.Appx.Psf;
using otor.msixhero.ui.Domain;
using otor.msixhero.ui.Modules.Common.PsfContent.ViewModel.Items;
using otor.msixhero.ui.Modules.Common.PsfContent.ViewModel.Items.Redirection;
using otor.msixhero.ui.Modules.Common.PsfContent.ViewModel.Items.Trace;

namespace otor.msixhero.ui.Modules.Common.PsfContent.ViewModel
{
    public class PsfContentViewModel : ChangeableContainer
    {
        public PsfContentViewModel(PsfConfig psfConfig)
        {
            this.RedirectionRules = new ChangeableCollection<PsfContentProcessRedirectionViewModel>();
            this.TraceRules = new ChangeableCollection<PsfContentProcessTraceViewModel>();

            this.Setup(psfConfig);

            this.RedirectionRules.Commit();
            this.TraceRules.Commit();
            this.AddChild(this.RedirectionRules);
        }

        public ChangeableCollection<PsfContentProcessRedirectionViewModel> RedirectionRules { get; }

        public ChangeableCollection<PsfContentProcessTraceViewModel> TraceRules { get; }

        private void Setup(PsfConfig psfConfig)
        {
            if (psfConfig?.Processes == null)
            {
                return;
            }

            foreach (var process in psfConfig.Processes)
            {
                foreach (var item in process.Fixups.Where(f => f.Config != null))
                {
                    if (item.Config is PsfRedirectionFixupConfig redirectionConfig)
                    {
                        var psfContentProcessViewModel = new PsfContentProcessRedirectionViewModel(process.Executable, item.Dll, redirectionConfig.RedirectedPaths);
                        this.RedirectionRules.Add(psfContentProcessViewModel);
                    }
                    else if (item.Config is PsfTraceFixupConfig traceConfig)
                    {
                        var psfContentProcessViewModel = new PsfContentProcessTraceViewModel(process.Executable, item.Dll, traceConfig);
                        this.TraceRules.Add(psfContentProcessViewModel);
                    }
                }
            }
        }
    }
}
