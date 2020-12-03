using System;
using System.Linq;
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
        
        public void RegisterForCommandLineGeneration(params IChangeableValue[] toSubscribe)
        {
            foreach (var item in toSubscribe)
            {
                item.ValueChanged += this.OnItemChanged;
            }
        }
        
        public void RegisterForCommandLineGeneration(params IChangeable[] toSubscribe)
        {
            this.RegisterForCommandLineGeneration(toSubscribe.OfType<IChangeableValue>().ToArray());
            
            foreach (var item in toSubscribe.OfType<ChangeableContainer>())
            {
                this.RegisterForCommandLineGeneration(item.GetChildren().ToArray());
            }
        }

        private void OnItemChanged(object sender, ValueChangedEventArgs e)
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