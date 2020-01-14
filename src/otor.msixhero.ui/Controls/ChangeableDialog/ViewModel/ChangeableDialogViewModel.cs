using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using otor.msixhero.lib.Infrastructure;
using otor.msixhero.lib.Infrastructure.Progress;
using otor.msixhero.ui.Commands.RoutedCommand;
using otor.msixhero.ui.Domain;
using Prism.Services.Dialogs;

namespace otor.msixhero.ui.Controls.ChangeableDialog.ViewModel
{
    public abstract class ChangeableDialogViewModel : ChangeableContainer, IDataErrorInfo, IDialogAware
    {
        private readonly IInteractionService interactionService;
        private string title;
        private bool showApplyButton;
        private ICommand okCommand;
        private DelegateCommand closeCommand;

        protected ChangeableDialogViewModel(string title, IInteractionService interactionService) : base(true)
        {
            this.showApplyButton = true;
            this.Title = title;
            this.interactionService = interactionService;
            this.SetValidationMode(ValidationMode.Silent, false);
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

        public string Error
        {
            get
            {
                if (this.ValidationMode == ValidationMode.Silent)
                {
                    return null;
                }

                return this.ValidationMessage;
            }
        }

        public bool HasError => this.Error != null;

        public override ValidationMode ValidationMode
        {
            get => base.ValidationMode;
            set
            {
                base.ValidationMode = value;
                this.OnPropertyChanged(nameof(Error));
                this.OnPropertyChanged(nameof(HasError));
            }
        }

        public override string ValidationMessage
        {
            get => base.ValidationMessage;
            protected set
            {
                var oldValue = this.ValidationMessage;
                base.ValidationMessage = value;

                if (oldValue != value)
                {
                    this.OnPropertyChanged(nameof(Error));
                    this.OnPropertyChanged(nameof(HasError));
                }
            }
        }

        public ICommand OkCommand => this.okCommand ??= new DelegateCommand(param => this.OkExecute(param is bool bp && bp), param => this.CanOkExecute(param is bool bp && bp));

        public DelegateCommand CloseCommand => this.closeCommand ??= new DelegateCommand(param => this.CloseExecute(param is ButtonResult result ? result : ButtonResult.Cancel), param => this.CanCloseExecute(param is ButtonResult result ? result : ButtonResult.Cancel));
        
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
            return this.IsValid || this.ValidationMode == ValidationMode.Silent;
        }

        public event Action<IDialogResult> RequestClose;


        private void OkExecute(bool closeWindow)
        {
            if (this.ValidationMode == ValidationMode.Silent)
            {
                this.SetValidationMode(ValidationMode.Default, true);
            }

            this.OnPropertyChanged(nameof(Error));
            this.OnPropertyChanged(nameof(HasError));

            if (!this.IsValid)
            {
                return;
            }

            var progress = new Progress<ProgressData>();

            EventHandler<ProgressData> handler = (sender, data) =>
            {
                this.State.Message = data.Message;
                this.State.Progress = data.Progress;
            };

            progress.ProgressChanged += handler;
            this.State.IsSaving = true;
            var task = this.Save(CancellationToken.None, progress);

            task.ContinueWith(t =>
                {
                    this.State.IsSaved = false;
                    progress.ProgressChanged -= handler;
                    this.State.IsSaving = false;
                    if (t.IsCanceled)
                    {
                        return;
                    }

                    if (t.IsFaulted)
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
            return this.CanSave();
        }

        private void CloseExecute(ButtonResult button)
        {
            if (this.RequestClose == null)
            {
                throw new NotSupportedException("This dialog does not support closing itself.");
            }

            this.RequestClose(new DialogResult(button));
        }

        // ReSharper disable once UnusedParameter.Local
        private bool CanCloseExecute(ButtonResult button)
        {
            return true;
        }
    }
}
