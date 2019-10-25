using System;
using Prism.Services.Dialogs;

namespace otor.msixhero.ui.Modules.Settings.ViewModel
{
    public class SettingsViewModel : IDialogAware
    {
        public bool CanCloseDialog()
        {
            return true;
        }

        public void OnDialogClosed()
        {
        }

        public void OnDialogOpened(IDialogParameters parameters)
        {
        }

        public string Title { get; }

        public event Action<IDialogResult> RequestClose;
    }
}
