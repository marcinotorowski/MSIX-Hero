// MSIX Hero
// Copyright (C) 2022 Marcin Otorowski
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// Full notice:
// https://github.com/marcinotorowski/msix-hero/blob/develop/LICENSE.md

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
        private readonly IInteractionService _interactionService;
        private string _title;
        private bool _showApplyButton;
        private ICommand _okCommand;
        private DelegateCommand<object> _closeCommand;
        private bool _showErrors;

        protected ChangeableDialogViewModel(string title, IInteractionService interactionService) : base(true)
        {
            this.displayValidationErrors = false;
            this._showApplyButton = true;
            this.Title = title.IndexOf("msix hero", StringComparison.OrdinalIgnoreCase) == -1 ? $"{title} - MSIX Hero" : title;
            this._interactionService = interactionService;
        }

        public string Title
        {
            get => _title;
            set => this.SetField(ref this._title, value);
        }

        public bool ShowApplyButton
        {
            get => _showApplyButton;
            set => this.SetField(ref this._showApplyButton, value);
        }

        public bool ShowErrors
        {
            get => _showErrors;
            protected set => this.SetField(ref this._showErrors, value);
        }

        public DialogState State { get; } = new DialogState();

        public new string Error => this.ValidationMessage;
        
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

        public ICommand OkCommand => this._okCommand ??= new DelegateCommand<object>(param => this.OkExecute(param is bool bp && bp), param => this.CanOkExecute(param is bool bp && bp));

        public DelegateCommand<object> CloseCommand => this._closeCommand ??= new DelegateCommand<object>(param => this.CloseExecute(param is ButtonResult result ? result : default(ButtonResult?)), param => this.CanCloseExecute(param is ButtonResult result ? result : default(ButtonResult?)));
        
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

            return !this.IsTouched || !this.IsValidated || !this.ShowErrors || this.IsValid;
        }

        public event Action<IDialogResult> RequestClose;

        private void OkExecute(bool closeWindow)
        {
            this.ShowErrors = true;

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
                        var result = this._interactionService.ShowError(exception.Message, exception);
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
