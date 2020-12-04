using System;
using System.Collections.Generic;
using Otor.MsixHero.Cli.Verbs;
using Otor.MsixHero.Infrastructure.Services;

namespace Otor.MsixHero.App.Mvvm.Changeable.Dialog.ViewModel
{
    public abstract class ChangeableAutomatedDialogViewModel : ChangeableDialogViewModel
    {
        protected ChangeableAutomatedDialogViewModel(string title, IInteractionService interactionService) : base(title, interactionService)
        {
        }

        public string SilentCommandLine => this.GenerateSilentCommandLine();

        protected abstract string GenerateSilentCommandLine();
        
        public void RegisterForCommandLineGeneration(params IChangeable[] toSubscribe)
        {
            this.RegisterForCommandLineGeneration(((IEnumerable<IChangeable>) toSubscribe));
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
            this.OnPropertyChanged(nameof(this.SilentCommandLine));
        }
    }

    public abstract class ChangeableAutomatedDialogViewModel<T> : ChangeableAutomatedDialogViewModel where T : BaseVerb
    {
        protected readonly T Verb = Activator.CreateInstance<T>();

        protected ChangeableAutomatedDialogViewModel(string title, IInteractionService interactionService) : base(title,
            interactionService)
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