using System;
using System.Collections.Generic;
using System.Text;
using Prism.Services.Dialogs;

namespace MSI_Hero.Modules.Settings.ViewModel
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
