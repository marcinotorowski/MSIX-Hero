using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using Otor.MsixHero.Infrastructure.Progress;
using Otor.MsixHero.Infrastructure.Services;
using Prism.Commands;
using Prism.Services.Dialogs;

namespace Otor.MsixHero.App.Mvvm.Changeable.Dialog.ViewModel
{
    public abstract class ChangeableDialogViewModel : ChangeableContainer, IDataErrorInfo, IDialogAware
    {
        private readonly IInteractionService interactionService;
        private string title;
        private bool showApplyButton;
        private ICommand okCommand;
        private DelegateCommand<object> closeCommand;

        protected ChangeableDialogViewModel(string title, IInteractionService interactionService) : base(true)
        {
            this.displayValidationErrors = false;
            this.showApplyButton = true;
            this.Title = title.IndexOf("msix hero", StringComparison.OrdinalIgnoreCase) == -1 ? $"{title} - MSIX Hero" : title;
            this.interactionService = interactionService;
        }

        public string Title
        {
            get => title;
            set => this.SetField(ref this.title, value);
        }

        public bool ShowApplyButton
        {
            get => showApplyButton;
            set => this.SetField(ref this.showApplyButton, value);
        }

        public DialogState State { get; } = new DialogState();

        public new string Error
        {
            get
            {
                if (!this.DisplayValidationErrors)
                {
                    return null;
                }

                return this.ValidationMessage;
            }
        }

        public override bool DisplayValidationErrors
        {
            get => base.DisplayValidationErrors;
            set
            {
                base.DisplayValidationErrors = value;
                this.OnPropertyChanged(nameof(Error));
            }
        }

        public bool HasError => this.Error != null;
        
        public override string ValidationMessage
        {
            get => base.ValidationMessage;
            protected set
            {
                base.ValidationMessage = value;
                this.OnPropertyChanged(nameof(HasError));
                this.OnPropertyChanged(nameof(Error));
            }
        }

        public ICommand OkCommand => this.okCommand ??= new DelegateCommand<object>(param => this.OkExecute(param is bool bp && bp), param => this.CanOkExecute(param is bool bp && bp));

        public DelegateCommand<object> CloseCommand => this.closeCommand ??= new DelegateCommand<object>(param => this.CloseExecute(param is ButtonResult result ? result : default(ButtonResult?)), param => this.CanCloseExecute(param is ButtonResult result ? result : default(ButtonResult?)));
        
        string IDataErrorInfo.this[string columnName] => null;

        protected abstract Task<bool> Save(CancellationToken cancellationToken, IProgress<ProgressData> progress);

        bool IDialogAware.CanCloseDialog()
        {
            return true;
        }

        void IDialogAware.OnDialogClosed()
        {
        }

        void IDialogAware.OnDialogOpened(IDialogParameters parameters)
        {
        }

        protected virtual bool CanSave()
        {
            if (this.State?.IsSaved == true)
            {
                return false;
            }

            return this.IsValid || !this.DisplayValidationErrors;
        }

        public event Action<IDialogResult> RequestClose;

        private void OkExecute(bool closeWindow)
        {
            if (!this.DisplayValidationErrors)
            {
                this.DisplayValidationErrors = true;
            }

            this.OnPropertyChanged(nameof(Error));
            this.OnPropertyChanged(nameof(HasError));

            if (!this.IsValid)
            {
                return;
            }

            var progress = new Progress<ProgressData>();
            var cancellationTokenSource = new CancellationTokenSource();

            var task = this.Save(cancellationTokenSource.Token, progress);
            this.State.Progress.MonitorProgress(task, cancellationTokenSource, progress);

            task.ContinueWith(t =>
                {
                    cancellationTokenSource.Dispose();
                    this.State.IsSaved = false;

                    CommandManager.InvalidateRequerySuggested();

                    if (t.IsCanceled)
                    {
                        return;
                    }

                    if (t.IsFaulted && t.Exception != null)
                    {
                        var exception = t.Exception.GetBaseException();
                        var result = this.interactionService.ShowError(exception.Message, exception);
                        if (result == InteractionResult.Retry)
                        {
                            this.OkExecute(closeWindow);
                        }

                        return;
                    }

                    if (!t.Result)
                    {
                        return;
                    }

                    this.State.IsSaved = t.IsCompleted;
                    this.State.WasSaved |= this.State.IsSaved;

                    if (closeWindow)
                    {
                        this.CloseCommand.Execute(ButtonResult.OK);
                    }
                },
                CancellationToken.None,
                TaskContinuationOptions.AttachedToParent | TaskContinuationOptions.ExecuteSynchronously,
                TaskScheduler.FromCurrentSynchronizationContext());
            /*
             * 
            e.CanExecute = !(this.DataContext is ChangeableDialogViewModel dataContext) || dataContext.CanSave();
        }

        private void SaveExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            if (this.DataContext is ChangeableDialogViewModel dataContext)
            {
                dataContext.Save(e.Parameter is bool boolParam && boolParam);
            }
        }

        private void CloseExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            if (this.DataContext is ChangeableDialogViewModel dataContext)
            {
                dataContext.Close(dataContext.State.IsSaved ? ButtonResult.OK : ButtonResult.Cancel);
            }
             */
        }

        // ReSharper disable once UnusedParameter.Local
        private bool CanOkExecute(bool closeWindow)
        {
            return this.State?.IsSaved != true && this.CanSave();
        }

        private void CloseExecute(ButtonResult? button)
        {
            if (this.RequestClose == null)
            {
                throw new NotSupportedException("This dialog does not support closing itself.");
            }
            
            if (button.HasValue)
            {
                this.RequestClose(new DialogResult(button.Value));
            }
            else if (this.State.WasSaved)
            {
                this.RequestClose(new DialogResult(ButtonResult.OK));
            }
            else
            {
                this.RequestClose(new DialogResult(ButtonResult.Cancel));
            }
        }

        // ReSharper disable once UnusedParameter.Local
        private bool CanCloseExecute(ButtonResult? button)
        {
            return true;
        }
    }
}
