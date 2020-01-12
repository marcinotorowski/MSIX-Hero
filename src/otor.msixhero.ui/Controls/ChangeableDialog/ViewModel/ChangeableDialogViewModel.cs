using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using otor.msixhero.lib.Infrastructure;
using otor.msixhero.lib.Infrastructure.Progress;
using otor.msixhero.ui.Domain;
using Prism.Services.Dialogs;

namespace otor.msixhero.ui.Controls.ChangeableDialog.ViewModel
{
    public abstract class ChangeableDialogViewModel : ChangeableContainer, IDataErrorInfo, IDialogAware
    {
        private readonly IInteractionService interactionService;
        private string title;
        private bool showApplyButton;

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

        public string this[string columnName] => null;
        
        protected abstract Task Save(CancellationToken cancellationToken, IProgress<ProgressData> progress);

        public virtual bool Save(bool closeOnSuccess)
        {
            if (this.ValidationMode == ValidationMode.Silent)
            {
                this.SetValidationMode(ValidationMode.Default, true);
            }

            this.OnPropertyChanged(nameof(Error));
            this.OnPropertyChanged(nameof(HasError));

            if (!this.IsValid)
            {
                return false;
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
                        this.Save(closeOnSuccess);
                    }

                    return;
                }

                this.State.IsSaved = t.IsCompleted;

                if (closeOnSuccess)
                {
                    this.Close(ButtonResult.OK);
                }
            }, 
            CancellationToken.None, 
            TaskContinuationOptions.AttachedToParent | TaskContinuationOptions.ExecuteSynchronously, 
            TaskScheduler.FromCurrentSynchronizationContext());

            return true;
        }

        public virtual bool CanSave()
        {
            return this.IsValid || this.ValidationMode == ValidationMode.Silent;
        }

        public void Close(ButtonResult buttonResult)
        {
            if (this.RequestClose == null)
            {
                throw new NotSupportedException("This dialog does not support closing itself.");
            }

            this.RequestClose(new DialogResult(buttonResult));
        }

        public virtual bool CanCloseDialog()
        {
            return true;
        }

        public virtual void OnDialogClosed()
        {
        }

        public virtual void OnDialogOpened(IDialogParameters parameters)
        {
        }

        public event Action<IDialogResult> RequestClose;
    }
}
