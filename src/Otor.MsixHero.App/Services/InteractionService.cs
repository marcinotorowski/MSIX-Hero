using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using Notifications.Wpf.Core;
using Ookii.Dialogs.Wpf;
using Otor.MsixHero.Infrastructure.Services;
using Application = System.Windows.Application;
using MessageBox = System.Windows.MessageBox;
using OpenFileDialog = Microsoft.Win32.OpenFileDialog;
using SaveFileDialog = Microsoft.Win32.SaveFileDialog;

namespace Otor.MsixHero.App.Services
{
    public class InteractionService : IInteractionService
    {
        private readonly INotificationManager notificationManager;
        private readonly SynchronizationContext context;

        public InteractionService(INotificationManager notificationManager)
        {
            this.notificationManager = notificationManager;
            this.context = SynchronizationContext.Current;
        }

        public InteractionResult Confirm(string body, string title = null, InteractionType type = InteractionType.Asterisk, InteractionButton buttons = InteractionButton.OK)
        {
            var targetType = (MessageBoxImage)(int)type;
            var targetButtons = (MessageBoxButton)(int)buttons;

            // ReSharper disable once AssignNullToNotNullAttribute
            var result = MessageBox.Show(Application.Current.MainWindow, body, title, targetButtons, targetType);
            return (InteractionResult)(int)result;
        }

        public Task ShowToast(string title, string message, InteractionType type = InteractionType.Information, Action clickCallback = null)
        {
            NotificationType toastType;

            switch (type)
            {
                case InteractionType.None:
                    toastType = NotificationType.Success;
                    break;
                case InteractionType.Error:
                    toastType = NotificationType.Error;
                    break;
                case InteractionType.Warning:
                    toastType = NotificationType.Warning;
                    break;
                default:
                    toastType = NotificationType.Information;
                    break;
            }

            if (clickCallback == null)
            {
                return this.notificationManager.ShowAsync(new NotificationContent()
                {
                    Message = message,
                    Type = toastType,
                    Title = title
                }, 
                "WindowArea");
            }

            return this.notificationManager.ShowAsync(new NotificationContent()
                {
                    Message = message,
                    Type = toastType,
                    Title = title
                },
                "WindowArea", 
                onClick: clickCallback);
        }

        public int ShowMessage(string body, IReadOnlyCollection<string> buttons, string title = null, string extendedInfo = null, InteractionResult systemButtons = InteractionResult.None)
        {
            var taskDialog = new TaskDialog
            {
                MainIcon = TaskDialogIcon.Information,
                ButtonStyle = TaskDialogButtonStyle.CommandLinks
            };

            foreach (var item in buttons)
            {
                var btn = new TaskDialogButton(ButtonType.Custom) { Text = item };
                taskDialog.Buttons.Add(btn);
            }

            if (systemButtons != InteractionResult.None)
            {
                var bts = new[] { InteractionResult.Yes, InteractionResult.No, InteractionResult.OK, InteractionResult.Cancel, InteractionResult.Close };
                foreach (var bt in bts)
                {
                    if ((systemButtons & bt) == bt)
                    {
                        taskDialog.Buttons.Add(new TaskDialogButton((ButtonType)(int)bt));
                    }
                }
            }

            taskDialog.CenterParent = true;
            taskDialog.Content = body;

            if (!string.IsNullOrEmpty(extendedInfo))
            {
                taskDialog.ExpandedInformation = extendedInfo;
            }

            taskDialog.WindowTitle = title ?? "MSIX Hero";

            int clickedIndex = -1;
            EventHandler<TaskDialogItemClickedEventArgs> handler = (sender, args) =>
            {
                var b = args.Item as TaskDialogButton;
                if (b == null)
                {
                    return;
                }

                clickedIndex = taskDialog.Buttons.IndexOf(b);
            };

            try
            {
                taskDialog.ButtonClicked += handler;

                if (this.context == null)
                {
                    taskDialog.ShowDialog(Application.Current.MainWindow);
                }

                var dispatcher = Application.Current.Dispatcher;
                if (dispatcher != null)
                {
                    dispatcher.Invoke(() =>
                    {
                        this.context.Send(
                            state => taskDialog.ShowDialog(Application.Current.MainWindow),
                            null);
                    },
                        DispatcherPriority.SystemIdle);
                }
            }
            finally
            {
                taskDialog.ButtonClicked -= handler;
            }

            return clickedIndex;
        }

        public InteractionResult ShowError(string body, Exception exception, InteractionResult buttons = InteractionResult.Close)
        {
#if DEBUG
            var extended = exception.ToString();
#else
            string extended = body != exception.Message ? exception.Message : null;
            if (exception.InnerException != null)
            {
                if (!string.IsNullOrEmpty(extended))
                {
                    extended += System.Environment.NewLine;
                    extended += exception.GetBaseException().Message;
                }
            }
#endif

            return this.ShowError(body, buttons, extendedInfo: extended);
        }

        public InteractionResult ShowError(string body, InteractionResult buttons = InteractionResult.Close, string title = null, string extendedInfo = null)
        {
            var taskDialog = new TaskDialog
            {
                MainIcon = TaskDialogIcon.Error,
                ButtonStyle = TaskDialogButtonStyle.Standard
            };

            if (buttons.HasFlag(InteractionResult.Retry))
            {
                taskDialog.Buttons.Add(new TaskDialogButton(ButtonType.Retry));
            }

            if (buttons.HasFlag(InteractionResult.OK))
            {
                taskDialog.Buttons.Add(new TaskDialogButton(ButtonType.Ok));
            }

            if (buttons.HasFlag(InteractionResult.Yes))
            {
                taskDialog.Buttons.Add(new TaskDialogButton(ButtonType.Yes));
            }

            if (buttons.HasFlag(InteractionResult.No))
            {
                taskDialog.Buttons.Add(new TaskDialogButton(ButtonType.No));
            }

            if (buttons.HasFlag(InteractionResult.Cancel))
            {
                taskDialog.Buttons.Add(new TaskDialogButton(ButtonType.Cancel));
            }

            if (buttons.HasFlag(InteractionResult.Close))
            {
                taskDialog.Buttons.Add(new TaskDialogButton(ButtonType.Close));
            }

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

            var dispatcher = Application.Current.Dispatcher;
            if (dispatcher != null)
            {
                dispatcher.Invoke(() =>
                {
                    this.context.Send(
                        state => result = (int)taskDialog.ShowDialog(Application.Current.MainWindow).ButtonType,
                        null);
                },
                DispatcherPriority.SystemIdle);
            }

            return (InteractionResult)result;
        }

        public InteractionResult ShowInfo(string body, InteractionResult buttons = InteractionResult.Close, string title = null, string extendedInfo = null)
        {
            var taskDialog = new TaskDialog
            {
                MainIcon = TaskDialogIcon.Information,
                ButtonStyle = TaskDialogButtonStyle.Standard
            };

            if (buttons.HasFlag(InteractionResult.Retry))
            {
                taskDialog.Buttons.Add(new TaskDialogButton(ButtonType.Retry));
            }

            if (buttons.HasFlag(InteractionResult.OK))
            {
                taskDialog.Buttons.Add(new TaskDialogButton(ButtonType.Ok));
            }

            if (buttons.HasFlag(InteractionResult.Yes))
            {
                taskDialog.Buttons.Add(new TaskDialogButton(ButtonType.Yes));
            }

            if (buttons.HasFlag(InteractionResult.No))
            {
                taskDialog.Buttons.Add(new TaskDialogButton(ButtonType.No));
            }

            if (buttons.HasFlag(InteractionResult.Cancel))
            {
                taskDialog.Buttons.Add(new TaskDialogButton(ButtonType.Cancel));
            }

            if (buttons.HasFlag(InteractionResult.Close))
            {
                taskDialog.Buttons.Add(new TaskDialogButton(ButtonType.Close));
            }

            taskDialog.CenterParent = true;
            taskDialog.Content = body;

            if (!string.IsNullOrEmpty(extendedInfo))
            {
                taskDialog.ExpandedInformation = extendedInfo;
            }

            taskDialog.WindowTitle = title ?? "MSIX Hero";

            if (this.context == null)
            {
                return (InteractionResult)(int)taskDialog.ShowDialog(Application.Current.MainWindow).ButtonType;
            }

            var result = 0;

            var dispatcher = Application.Current.Dispatcher;
            if (dispatcher != null)
            {
                dispatcher.Invoke(() =>
                {
                    this.context.Send(
                        state => result = (int)taskDialog.ShowDialog(Application.Current.MainWindow).ButtonType,
                        null);
                },
                DispatcherPriority.SystemIdle);
            }

            return (InteractionResult)result;
        }

        public bool SelectFile(string initialFile, string filterString, out string selectedFile)
        {
            if (!SelectFile(initialFile, filterString, false, out var selection))
            {
                selectedFile = null;
                return false;
            }

            selectedFile = selection.FirstOrDefault();
            return true;
        }

        public bool SaveFile(string initialFile, string filterString, out string selectedFile)
        {
            var dlg = new SaveFileDialog
            {
                Filter = PrepareFilterString(filterString)
            };

            if (initialFile != null)
            {
                var fileInfo = new FileInfo(initialFile);
                if (fileInfo.Directory != null && fileInfo.Directory.Exists)
                {
                    dlg.InitialDirectory = fileInfo.Directory.FullName;
                }

                dlg.FileName = fileInfo.Name;
            }

            dlg.CheckFileExists = false;
            var result = dlg.ShowDialog() == true;
            selectedFile = dlg.FileName;
            return result;
        }

        public bool SaveFile(string filterString, out string selectedFile)
        {
            return this.SaveFile(null, filterString, out selectedFile);
        }

        public bool SaveFile(out string selectedFile)
        {
            return this.SaveFile(null, null, out selectedFile);
        }

        public bool SelectFile(string filterString, out string selectedFile)
        {
            return this.SelectFile(null, filterString, out selectedFile);
        }

        public bool SelectFile(out string selectedFile)
        {
            return this.SelectFile(null, "All files (*.*)|*.*", out selectedFile);
        }

        public bool SelectFiles(string initialFile, string filterString, out string[] selectedFiles)
        {
            return SelectFile(initialFile, filterString, true, out selectedFiles);
        }

        public bool SelectFiles(string filterString, out string[] selectedFiles)
        {
            return SelectFile(null, filterString, true, out selectedFiles);
        }

        public bool SelectFolder(string initialFolder, out string selectedFolder)
        {
            var dlg = new VistaFolderBrowserDialog
            {
                SelectedPath = initialFolder
            };

            var result = dlg.ShowDialog() == true;
            selectedFolder = dlg.SelectedPath;
            return result;
        }

        public bool SelectFolder(out string selectedFolder)
        {
            return this.SelectFolder(null, out selectedFolder);
        }

        private static bool SelectFile(string initialFile, string filterString, bool withMultiSelection, out string[] selectedFiles)
        {
            var dlg = new OpenFileDialog();

            dlg.Filter = PrepareFilterString(filterString);
            if (initialFile != null)
            {
                var fileInfo = new FileInfo(initialFile);
                if (fileInfo.Directory != null && fileInfo.Directory.Exists)
                {
                    dlg.InitialDirectory = fileInfo.Directory.FullName;
                }

                dlg.FileName = fileInfo.FullName;
            }

            dlg.CheckFileExists = true;
            dlg.Multiselect = withMultiSelection;

            var result = dlg.ShowDialog() == true;
            selectedFiles = dlg.FileNames;
            return result;
        }

        private static string PrepareFilterString(string filterString)
        {
            if (string.IsNullOrEmpty(filterString))
            {
                return "All files (*.*)|*.*";
            }

            var items = filterString.Split('|');
            if (items.Length > 2)
            {
                var sb = new StringBuilder();
                var supportedPatterns = new List<string>();

                var hasAll = false;
                for (var i = 0; i + 1 < items.Length; i += 2)
                {
                    var first = items[i]; // the caption
                    var second = items[i + 1]; // the filter

                    if (!supportedPatterns.Contains(second) && second != "*.*")
                    {
                        supportedPatterns.Add(second);
                    }

                    if (sb.Length > 0)
                    {
                        sb.Append('|');
                    }

                    if (second.Contains("*.*"))
                    {
                        hasAll = true;
                    }

                    sb.Append(first);
                    sb.Append('|');
                    sb.Append(second);
                }

                if (!hasAll)
                {
                    sb.Append("|All files|*.*");
                }

                if (supportedPatterns.Count > 1)
                {
                    var supported = string.Join(";", supportedPatterns);
                    sb.Insert(0, "All supported files|" + supported + "|");
                }

                filterString = sb.ToString();
            }
            else if (items.Length == 1 && items[0].StartsWith("*.", StringComparison.Ordinal))
            {
                if (items[0] == "*.*")
                {
                    filterString = "All files|*.*";
                }
                else
                {
                    var fileExt = items[0].Substring(2);
                    filterString = $"{fileExt.ToUpperInvariant()} files ({filterString})|{filterString}|All files|*.*";
                }
            }

            return filterString;
        }
    }
}