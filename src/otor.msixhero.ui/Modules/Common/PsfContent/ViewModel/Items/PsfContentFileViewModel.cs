using System;
using System.Windows.Forms;
using otor.msixhero.ui.Modules.Common.PsfContent.View;

namespace otor.msixhero.ui.Modules.Common.PsfContent.ViewModel.Items
{
    public class PsfContentFileViewModel : PsfContentRegexpViewModel
    {
        public PsfContentFileViewModel(string regularExpression, bool isExclusion = false) : base(regularExpression)
        {
            this.IsExclusion = isExclusion;
        }

        public bool IsExclusion { get; }
    }
}
