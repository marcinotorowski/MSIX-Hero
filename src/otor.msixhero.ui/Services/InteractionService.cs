using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Ookii.Dialogs.Wpf;
using otor.msixhero.lib.Services;
using Application = System.Windows.Application;

namespace otor.msixhero.ui.Services
{
    public class InteractionService : IInteractionService
    {
        private readonly SynchronizationContext context;

        public InteractionService()
        {
            this.context = SynchronizationContext.Current;
        }

        public InteractionResult Confirm(string body, string title = null, InteractionType type = InteractionType.Asterisk, InteractionButton buttons = InteractionButton.OK)
        {
            var targetType = (MessageBoxIcon)(int)type;
            var targetButtons = (MessageBoxButtons)(int)buttons;

            var result = MessageBox.Show(body, title, targetButtons, targetType);
            return (InteractionResult)(int)result;
        }

        public InteractionResult ShowError(string body, string title = null, string extendedInfo = null)
        {
            var taskDialog = new TaskDialog();
            taskDialog.MainIcon = TaskDialogIcon.Error;
            taskDialog.ButtonStyle = TaskDialogButtonStyle.Standard;
            taskDialog.Buttons.Add(new TaskDialogButton(ButtonType.Retry)); 
            taskDialog.Buttons.Add(new TaskDialogButton(ButtonType.Close));

            taskDialog.CenterParent = true;
            taskDialog.Content = body;

            if (!string.IsNullOrEmpty(extendedInfo))
            {
                taskDialog.ExpandedInformation = extendedInfo;
            }

            taskDialog.WindowTitle = title ?? "MSIX Hero - Error";

            if (this.context == null)
            {
                return (InteractionResult)(int)taskDialog.ShowDialog(Application.Current.MainWindow).ButtonType;
            }

            var result = 0;
            this.context.Send(state => result = (int)taskDialog.ShowDialog(Application.Current.MainWindow).ButtonType, null);
            return (InteractionResult) result;
        }

        public bool SelectFile(string initialFile, string filterString, out string selectedFile)
        {
            var dlg = new Ookii.Dialogs.Wpf.VistaOpenFileDialog();

            dlg.Filter = filterString;

            if (initialFile != null && File.Exists(initialFile))
            {
                dlg.InitialDirectory = System.IO.Path.GetDirectoryName(initialFile);
                dlg.FileName = System.IO.Path.GetFileName(initialFile);
            }

            var result = dlg.ShowDialog() == true;
            selectedFile = dlg.FileName;
            return result;
        }

        public bool SelectFile(string filterString, out string selectedFile)
        {
            return this.SelectFile(null, filterString, out selectedFile);
        }

        public bool SelectFile(out string selectedFile)
        {
            return this.SelectFile(null, "*.*|All files (*.*)", out selectedFile);
        }

        public bool SelectFolder(string initialFolder, out string selectedFolder)
        {
            var dlg = new Ookii.Dialogs.Wpf.VistaFolderBrowserDialog();
            dlg.SelectedPath = initialFolder;
            var result = dlg.ShowDialog() == true;
            selectedFolder = dlg.SelectedPath;
            return result;
        }

        public bool SelectFolder(out string selectedFolder)
        {
            return this.SelectFolder(null, out selectedFolder);
        }
    }
}
