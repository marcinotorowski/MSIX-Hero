using System;
using System.Collections.Generic;
using System.Linq;
using otor.msixhero.ui.Domain;

namespace otor.msixhero.ui.Modules.Dialogs.PsfExpert.ViewModel
{
    public class PsfExpertRuleConditionViewModel : ChangeableContainer
    {
        public PsfExpertRuleConditionViewModel(IEnumerable<PsfExpertFilePatternViewModel> positive, IEnumerable<PsfExpertFilePatternViewModel> negative, PsfExpertThenViewModel then)
        {
            this.Positive = new ChangeableCollection<PsfExpertFilePatternViewModel>();
            this.Negative = new ChangeableCollection<PsfExpertFilePatternViewModel>();

            this.Positive = new ChangeableCollection<PsfExpertFilePatternViewModel>(positive);
            if (this.Positive.Any())
            {
                this.Positive.First().IsFirst = true;
            }

            this.Negative = new ChangeableCollection<PsfExpertFilePatternViewModel>(negative);
            this.Then = then;

            this.Positive.Commit();
            this.Negative.Commit();

            this.AddChildren(this.Positive, this.Negative, then);
        }
        
        public ChangeableCollection<PsfExpertFilePatternViewModel> Positive { get; }

        public ChangeableCollection<PsfExpertFilePatternViewModel> Negative { get; }

        public PsfExpertThenViewModel Then { get; }

        public override string ToString()
        {
            return "If";
        }
    }
}