using otor.msixhero.ui.ViewModel;

namespace otor.msixhero.ui.Controls.ChangeableDialog.ViewModel
{
    public class DialogState : NotifyPropertyChanged
    {
        private bool isSaving;
        private bool isSaved;
        private bool wasSaved;
        private string progressMessage;
        private int progressValue;
        
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

        public bool IsSaving
        {
            get => this.isSaving;
            set => this.SetField(ref this.isSaving, value);
        }

        public string Message
        {
            get => this.progressMessage;
            set => this.SetField(ref this.progressMessage, value);
        }

        public int Progress
        {
            get => this.progressValue;
            set => this.SetField(ref this.progressValue, value);
        }
    }
}