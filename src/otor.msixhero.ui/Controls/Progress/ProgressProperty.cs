using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using Otor.MsixHero.Infrastructure.Progress;
using Otor.MsixHero.Ui.Commands.RoutedCommand;
using Otor.MsixHero.Ui.ViewModel;

namespace Otor.MsixHero.Ui.Controls.Progress
{
    public class ProgressProperty : NotifyPropertyChanged
    {
        private bool isLoading;
        private int progress;
        private string message;
        private string error;
        private bool hasError;
        private bool supportsCancelling;
        private ICommand cancel;
        private CancellationTokenSource currentCancellationTokenSource;
        private bool isCancelling;

        public bool IsLoading
        {
            get => this.isLoading;
            set => this.SetField(ref this.isLoading, value);
        }

        public string Message
        {
            get => this.message;
            set => this.SetField(ref this.message, value);
        }

        public int Progress
        {
            get => this.progress;
            set => this.SetField(ref this.progress, value);
        }

        public string Error
        {
            get => this.error;
            set => this.SetField(ref this.error, value);
        }

        public bool HasError
        {
            get => this.hasError;
            set => this.SetField(ref this.hasError, value);
        }

        public bool SupportsCancelling
        {
            get => this.supportsCancelling;
            set => this.SetField(ref this.supportsCancelling, value);
        }

        public ICommand Cancel
        {
            get => this.cancel ??= new DelegateCommand(this.ExecuteCancel, this.CanExecuteCancel);
        }

        public void MonitorProgress(Task task, CancellationTokenSource cancellationTokenSource = default, IProgress<ProgressData> progressReporter = default, string initialMessage = default)
        {
            this.Progress = -1;
            this.isCancelling = false;
            this.currentCancellationTokenSource = cancellationTokenSource;
            this.SupportsCancelling = cancellationTokenSource != default;

            CommandManager.InvalidateRequerySuggested();
            this.IsLoading = true;

            if (initialMessage != null && !this.isCancelling)
            {
                this.Message = initialMessage;
            }
            else
            {
                this.Message = "Please wait...";
            }
            
            EventHandler<ProgressData> progressChanged = (sender, args) =>
            {
                if (this.isCancelling)
                {
                    return;
                }

                this.Progress = args.Progress;
                this.Message = args.Message;
            };

            var progressListen = progressReporter as Progress<ProgressData>;

            if (progressListen != null)
            {
                progressListen.ProgressChanged -= progressChanged;
                progressListen.ProgressChanged += progressChanged;
            }

            task.ContinueWith(t =>
            {
                this.currentCancellationTokenSource = null;
                this.isCancelling = false;
                this.HasError = t.IsFaulted;
                
                if (t.IsFaulted && t.Exception != null)
                {
                    this.Error = t.Exception.GetBaseException().Message;
                }
                else
                {
                    this.Error = null;
                }

                if (progressListen != null)
                {
                    progressListen.ProgressChanged -= progressChanged;
                }

                this.IsLoading = false;
            });
        }

        private void ExecuteCancel(object obj)
        {
            if (this.isCancelling)
            {
                return;
            }

            this.isCancelling = true;
            if (this.currentCancellationTokenSource.IsCancellationRequested)
            {
                return;
            }

            this.Progress = -1;
            this.Message = "Cancelling...";

            this.currentCancellationTokenSource?.Cancel();
        }

        private bool CanExecuteCancel(object obj)
        {
            return !this.isCancelling;
        }
    }
}
