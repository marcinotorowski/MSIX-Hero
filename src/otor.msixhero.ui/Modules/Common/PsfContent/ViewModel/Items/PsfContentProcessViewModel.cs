using System;
using otor.msixhero.lib.Domain.Appx.Psf;
using otor.msixhero.ui.Domain;
using otor.msixhero.ui.Modules.Common.PsfContent.View;

namespace otor.msixhero.ui.Modules.Common.PsfContent.ViewModel.Items
{
    public abstract class PsfContentProcessViewModel : PsfContentRegexpViewModel
    {
        private readonly ChangeableProperty<bool> is64Bit;

        protected PsfContentProcessViewModel(string processRegularExpression, string fixupName) : base(processRegularExpression)
        {
            this.is64Bit = new ChangeableProperty<bool>(fixupName.IndexOf("64", StringComparison.Ordinal) != -1);
            this.AddChildren(this.is64Bit);
        }

        public bool Is64Bit
        {
            get => this.is64Bit.CurrentValue;
            set
            {
                if (this.is64Bit.CurrentValue == value)
                {
                    return;
                }

                this.is64Bit.CurrentValue = value;
                this.OnPropertyChanged();
            }
        }

        protected sealed override void SetValuesFromRegularExpression(string expr)
        {
            var interpretation = new RegexpInterpreter(expr, false);

            switch (interpretation.Result)
            {
                case InterpretationResult.Any:
                    this.TextBefore = null;
                    this.DisplayText = "any ";
                    this.TextAfter = "process";
                    break;
                case InterpretationResult.Name:
                    this.TextBefore = "process ";
                    this.DisplayText = interpretation.DisplayText;
                    this.TextAfter = null;
                    break;
                case InterpretationResult.StartsWith:
                    this.TextBefore = "processes that start with ";
                    this.DisplayText = interpretation.DisplayText;
                    this.TextAfter = null;
                    break;
                case InterpretationResult.EndsWith:
                    this.TextBefore = "processes that end with ";
                    this.DisplayText = interpretation.DisplayText;
                    this.TextAfter = null;
                    break;
                default:
                    this.TextBefore = "processes that match pattern ";
                    this.DisplayText = interpretation.RegularExpression;
                    this.TextAfter = null;
                    break;
            }
        }
    }
}