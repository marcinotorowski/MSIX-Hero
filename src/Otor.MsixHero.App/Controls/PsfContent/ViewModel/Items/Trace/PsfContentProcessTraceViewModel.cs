using Otor.MsixHero.App.Changeable;
using Otor.MsixHero.Appx.Psf.Entities;

namespace Otor.MsixHero.App.Controls.PsfContent.ViewModel.Items.Trace
{
    public class PsfContentProcessTraceViewModel : PsfContentProcessViewModel
    {
        private readonly ChangeableProperty<TraceLevel> breakOn;
        private readonly ChangeableProperty<string> traceMethod;

        public PsfContentProcessTraceViewModel(string processRegularExpression, string fixupName, PsfTraceFixupConfig traceFixup) : base(processRegularExpression, fixupName)
        {
            this.breakOn = new ChangeableProperty<TraceLevel>(traceFixup.BreakOn);
            this.traceMethod = new ChangeableProperty<string>(traceFixup.TraceMethod);
            this.TraceLevel = new PsfContentTraceLevelViewModel(traceFixup.TraceLevels);

            this.AddChildren(this.breakOn, this.traceMethod, this.TraceLevel);
        }

        public PsfContentTraceLevelViewModel TraceLevel { get; }

        public string TraceMethod
        {
            get => this.traceMethod.CurrentValue;
            set
            {
                if (this.traceMethod.CurrentValue == value)
                {
                    return;
                }

                this.traceMethod.CurrentValue = value;
                this.OnPropertyChanged();
            }
        }

        public TraceLevel BreakOn
        {
            get => this.breakOn.CurrentValue;
            set
            {
                if (this.breakOn.CurrentValue == value)
                {
                    return;
                }

                this.breakOn.CurrentValue = value;
                this.OnPropertyChanged();
            }
        }
    }
}
