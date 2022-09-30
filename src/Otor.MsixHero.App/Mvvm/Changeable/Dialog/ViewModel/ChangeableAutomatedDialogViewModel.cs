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
using System.Collections.Generic;
using Otor.MsixHero.Cli.Verbs;
using Otor.MsixHero.Infrastructure.Helpers;
using Otor.MsixHero.Infrastructure.Services;

namespace Otor.MsixHero.App.Mvvm.Changeable.Dialog.ViewModel
{
    public abstract class ChangeableAutomatedDialogViewModel : ChangeableDialogViewModel
    {
        private bool _commandLineSupressed;

        protected ChangeableAutomatedDialogViewModel(string title, IInteractionService interactionService) : base(title, interactionService)
        {
        }

        public string SilentCommandLine => ExceptionGuard.Guard(this.GenerateSilentCommandLine);

        protected abstract string GenerateSilentCommandLine();
        
        public void RegisterForCommandLineGeneration(params IChangeable[] toSubscribe)
        {
            this.RegisterForCommandLineGeneration(((IEnumerable<IChangeable>) toSubscribe));
        }

        protected void DoBulkChange(Action toExecute)
        {
            this._commandLineSupressed = true;
            try
            {
                toExecute();
            }
            finally
            {
                this._commandLineSupressed = false;
                this.OnPropertyChanged(nameof(this.SilentCommandLine));
            }
        }

        public void RegisterForCommandLineGeneration(IEnumerable<IChangeable> toSubscribe)
        {
            foreach (var item in toSubscribe)
            {
                item.Changed += this.OnItemChanged;
            }
        }

        private void OnItemChanged(object sender, EventArgs e)
        {
            if (this._commandLineSupressed)
            {
                return;
            }

            this.OnPropertyChanged(nameof(this.SilentCommandLine));
        }
    }

    public abstract class ChangeableAutomatedDialogViewModel<T> : ChangeableAutomatedDialogViewModel where T : BaseVerb
    {
        protected readonly T Verb = Activator.CreateInstance<T>();
        
        protected ChangeableAutomatedDialogViewModel(string title, IInteractionService interactionService) : base(title, interactionService)
        {
        }

        protected abstract void UpdateVerbData();
        
        protected sealed override string GenerateSilentCommandLine()
        {
            this.UpdateVerbData();
            return this.Verb.ToCommandLineString();
        }
    }
}