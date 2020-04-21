using otor.msixhero.lib.Domain.Appx.Psf;

namespace otor.msixhero.ui.Modules.Common.PsfContent.ViewModel.Items.Electron
{
    public class PsfContentProcessElectronViewModel : PsfContentProcessViewModel
    {
        public PsfContentProcessElectronViewModel(string processRegularExpression, string fixupName, PsfElectronFixupConfig traceFixup) : base(processRegularExpression, fixupName)
        {
        }
    }
}
