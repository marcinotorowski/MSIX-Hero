using otor.msixhero.ui.Domain;

namespace otor.msixhero.ui.Modules.Dialogs.PsfExpert.ViewModel
{
    public class PsfExpertThenViewModel : ChangeableContainer
    {
        private readonly ChangeableProperty<bool> isReadOnly;
        private readonly ChangeableProperty<string> redirectTargetBase;

        public PsfExpertThenViewModel(bool isReadOnly, string redirectTargetBase)
        {
            this.isReadOnly = new ChangeableProperty<bool>(isReadOnly);
            this.redirectTargetBase = new ChangeableProperty<string>(redirectTargetBase);
            this.AddChildren(this.isReadOnly, this.redirectTargetBase);
        }

        public bool IsReadOnly
        {
            get => this.isReadOnly.CurrentValue;
            set
            {
                if (this.isReadOnly.CurrentValue == value)
                {
                    return;
                }

                this.isReadOnly.CurrentValue = value;
                this.OnPropertyChanged();
            }
        }
        
        public string RedirectTargetBase
        {
            get => this.redirectTargetBase.CurrentValue;
            set
            {
                if (this.redirectTargetBase.CurrentValue == value)
                {
                    return;
                }

                this.redirectTargetBase.CurrentValue = value;
                this.OnPropertyChanged();
            }
        }

        public override string ToString()
        {
            if (string.IsNullOrEmpty(this.RedirectTargetBase))
            {
                return "THEN Redirect to <LocalAppData>" + (this.IsReadOnly ? " (read-only)" : string.Empty);
            }

            return "THEN Redirect to " + this.RedirectTargetBase + (this.IsReadOnly ? " (read-only)" : string.Empty);
        }
    }
}