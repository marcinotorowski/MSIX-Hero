namespace Otor.MsixHero.App.Controls.PsfContent.ViewModel.Items.Redirection
{
    public class PsfContentFileRedirectionViewModel : PsfContentRegexpViewModel
    {
        public PsfContentFileRedirectionViewModel(string regularExpression, bool isExclusion = false) : base(regularExpression)
        {
            this.IsExclusion = isExclusion;
        }

        public bool IsExclusion { get; }
    }
}
