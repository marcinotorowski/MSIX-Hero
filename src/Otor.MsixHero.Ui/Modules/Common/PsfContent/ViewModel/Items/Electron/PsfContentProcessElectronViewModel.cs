using Otor.MsixHero.Appx.Psf.Entities;

namespace Otor.MsixHero.Ui.Modules.Common.PsfContent.ViewModel.Items.Electron
{
    public class PsfContentProcessElectronViewModel : PsfContentProcessViewModel
    {
        public PsfContentProcessElectronViewModel(string processRegularExpression, string fixupName, PsfElectronFixupConfig traceFixup) : base(processRegularExpression, fixupName)
        {
        }
    }
}
