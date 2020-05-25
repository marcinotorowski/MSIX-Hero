using otor.msixhero.ui.Controls.Progress;
using otor.msixhero.ui.ViewModel;

namespace otor.msixhero.ui.Controls.ChangeableDialog.ViewModel
{
    public class DialogState : NotifyPropertyChanged
    {
        private bool isSaved;
        private bool wasSaved;
        
        public bool IsSaved
        {
            get => this.isSaved;
            set => this.SetField(ref this.isSaved, value);
        }

        public bool WasSaved
        {
            get => this.IsSaved || this.wasSaved;
            set => this.SetField(ref this.wasSaved, value);
        }

        public ProgressProperty Progress { get; } = new ProgressProperty();
    }
}