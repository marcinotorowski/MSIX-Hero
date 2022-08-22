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
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using Otor.MsixHero.Infrastructure.Progress;
using Prism.Commands;

namespace Otor.MsixHero.App.Mvvm
{
    public class ProgressProperty : NotifyPropertyChanged
    {
        private bool _isLoading;
        private int _progress;
        private string _message;
        private string _error;
        private bool _hasError;
        private bool _supportsCancelling;
        private ICommand _cancel;
        private CancellationTokenSource _currentCancellationTokenSource;
        private bool _isCancelling;

        public bool IsLoading
        {
            get => this._isLoading;
            set => this.SetField(ref this._isLoading, value);
        }

        public string Message
        {
            get => this._message;
            set => this.SetField(ref this._message, value);
        }

        public int Progress
        {
            get => this._progress;
            set => this.SetField(ref this._progress, value);
        }

        public string Error
        {
            get => this._error;
            set => this.SetField(ref this._error, value);
        }

        public bool HasError
        {
            get => this._hasError;
            set => this.SetField(ref this._hasError, value);
        }

        public bool SupportsCancelling
        {
            get => this._supportsCancelling;
            set => this.SetField(ref this._supportsCancelling, value);
        }

        public ICommand Cancel => this._cancel ??= new DelegateCommand(this.ExecuteCancel, this.CanExecuteCancel);

        public void MonitorProgress(Task task, CancellationTokenSource cancellationTokenSource = default, IProgress<ProgressData> progressReporter = default, string initialMessage = default)
        {
            this.Progress = -1;
            this._isCancelling = false;
            this._currentCancellationTokenSource = cancellationTokenSource;
            this.SupportsCancelling = cancellationTokenSource != default;

            CommandManager.InvalidateRequerySuggested();
            this.IsLoading = true;

            if (initialMessage != null && !this._isCancelling)
            {
                this.Message = initialMessage;
            }
            else
            {
                this.Message = Resources.Localization.Loading_PleaseWait;
            }

            // ReSharper disable once ConvertToLocalFunction
            EventHandler<ProgressData> progressChanged = (_, args) =>
            {
                if (this._isCancelling)
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
                this._currentCancellationTokenSource = null;
                this._isCancelling = false;
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

        private void ExecuteCancel()
        {
            if (this._isCancelling)
            {
                return;
            }

            this._isCancelling = true;
            if (this._currentCancellationTokenSource?.IsCancellationRequested == true)
            {
                return;
            }

            this.Progress = -1;
            this.Message = Resources.Localization.Loading_Cancelling;

            this._currentCancellationTokenSource?.Cancel();
        }

        private bool CanExecuteCancel()
        {
            return !this._isCancelling;
        }
    }

}
