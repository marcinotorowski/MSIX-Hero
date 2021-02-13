namespace Otor.MsixHero.Infrastructure.Services
{
    public struct FileDialogSettings
    {
        public FileDialogSettings(string filter, string dialogTitle = null, string initialSelection = null)
        {
            Filter = filter;
            DialogTitle = dialogTitle;
            InitialSelection = initialSelection;
        }

        public string Filter { get; set; }

        public string DialogTitle { get; set; }

        public string InitialSelection { get; set; }

        public static FileDialogSettings FromFilterString(string filter)
        {
            return new FileDialogSettings(filter);
        }
    }
}
