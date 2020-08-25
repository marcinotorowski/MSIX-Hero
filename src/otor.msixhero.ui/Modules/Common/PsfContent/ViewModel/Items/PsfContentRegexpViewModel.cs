using Otor.MsixHero.Ui.Domain;
using Otor.MsixHero.Ui.Modules.Common.PsfContent.Model;

namespace Otor.MsixHero.Ui.Modules.Common.PsfContent.ViewModel.Items
{
    public abstract class PsfContentRegexpViewModel : ChangeableContainer
    {
        // ReSharper disable once InconsistentNaming
        protected ChangeableProperty<string> regularExpression;
        private string displayText;
        private string textBefore;
        private string textAfter;

        protected PsfContentRegexpViewModel(string regularExpression)
        {
            this.regularExpression = new ChangeableProperty<string>(regularExpression);
            this.regularExpression.ValueChanged += this.RegularExpressionOnValueChanged;
            this.SetValuesFromRegularExpression(regularExpression);
            this.AddChild(this.regularExpression);
        }

        public string RegularExpression
        {
            get => this.regularExpression.CurrentValue;
            set
            {
                if (this.regularExpression.CurrentValue == value)
                {
                    return;
                }

                this.regularExpression.CurrentValue = value;
                this.OnPropertyChanged(nameof(this.RegularExpression));
            }
        }

        public string DisplayText
        {
            get => this.displayText;
            protected set => this.SetField(ref this.displayText, value);
        }

        public string TextBefore
        {
            get => this.textBefore;
            protected set => this.SetField(ref this.textBefore, value);
        }

        public string TextAfter
        {
            get => this.textAfter;
            protected set => this.SetField(ref this.textAfter, value);
        }
        
        protected virtual void SetValuesFromRegularExpression(string expr)
        {
            var interpretation = new RegexpInterpreter(expr, true);

            switch (interpretation.Result)
            {
                case InterpretationResult.Any:
                    this.TextBefore = null;
                    this.DisplayText = "All files";
                    this.TextAfter = null;
                    break;
                case InterpretationResult.Extension:
                    this.TextBefore = "Files with extension ";
                    this.DisplayText = interpretation.DisplayText;
                    this.TextAfter = null;
                    break;
                case InterpretationResult.Name:
                    this.TextBefore = "File ";
                    this.DisplayText = interpretation.DisplayText;
                    this.TextAfter = null;
                    break;
                case InterpretationResult.StartsWith:
                    this.TextBefore = "Files that start with ";
                    this.DisplayText = interpretation.DisplayText;
                    this.TextAfter = null;
                    break;
                case InterpretationResult.EndsWith:
                    this.TextBefore = "Files that end with ";
                    this.DisplayText = interpretation.DisplayText;
                    this.TextAfter = null;
                    break;
                default:
                    this.TextBefore = "Files that match pattern";
                    this.DisplayText = interpretation.RegularExpression;
                    this.TextAfter = null;
                    break;
            }
        }

        private void RegularExpressionOnValueChanged(object sender, ValueChangedEventArgs e)
        {
            this.SetValuesFromRegularExpression((string)e.NewValue);
        }
    }
}