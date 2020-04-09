using System;
using System.Collections.Generic;
using otor.msixhero.lib.Domain.Appx.Psf;
using otor.msixhero.ui.Domain;

namespace otor.msixhero.ui.Modules.Dialogs.PsfExpert.ViewModel.Items
{
    public enum PsfExpertRuleConditionRedirectionMode
    {
        X64,

        X86,

        Custom
    }

    public class PsfExpertRedirectionIfProcessViewModel : ChangeableContainer
    {
        private readonly ChangeableProperty<string> fixupName;
        private readonly ChangeableProperty<string> processName;
        private readonly ChangeableProperty<PsfExpertRuleConditionRedirectionMode> fixupMode;

        public PsfExpertRedirectionIfProcessViewModel(
            string fixupName,
            PsfProcess process,
            IEnumerable<PsfExpertRuleConditionViewModel> directoryConditions)
        {
            if (string.IsNullOrEmpty(fixupName))
            {
                this.fixupMode = new ChangeableProperty<PsfExpertRuleConditionRedirectionMode>(PsfExpertRuleConditionRedirectionMode.Custom);
            }
            else if (fixupName.IndexOf("86", StringComparison.Ordinal) != -1 || fixupName.IndexOf("32", StringComparison.Ordinal) != -1)
            {
                this.fixupMode = new ChangeableProperty<PsfExpertRuleConditionRedirectionMode>(PsfExpertRuleConditionRedirectionMode.X86);
            }
            else if (fixupName.IndexOf("64", StringComparison.Ordinal) != -1)
            {
                this.fixupMode = new ChangeableProperty<PsfExpertRuleConditionRedirectionMode>(PsfExpertRuleConditionRedirectionMode.X64);
            }
            else
            {
                this.fixupMode = new ChangeableProperty<PsfExpertRuleConditionRedirectionMode>(PsfExpertRuleConditionRedirectionMode.Custom);
            }

            this.fixupName = new ChangeableProperty<string>(fixupName);
            this.BaseObject = process;
            this.processName = new ChangeableProperty<string>(process.Executable);
            this.DirectoryCondition = new ChangeableCollection<PsfExpertRuleConditionViewModel>(directoryConditions);
            this.DirectoryCondition.Commit();

            this.AddChildren(this.processName, this.DirectoryCondition, this.fixupMode);
        }

        public string FixupName
        {
            get => this.fixupName.CurrentValue;
            set
            {
                if (this.fixupName.CurrentValue == value)
                {
                    return;
                }

                this.fixupName.CurrentValue = value;
                this.OnPropertyChanged();
            }
        }


        public PsfExpertRuleConditionRedirectionMode FixupMode
        {
            get => this.fixupMode.CurrentValue;
            set
            {
                if (this.fixupMode.CurrentValue == value)
                {
                    return;
                }

                this.fixupMode.CurrentValue = value;
                this.OnPropertyChanged();
            }
        }

        public ChangeableCollection<PsfExpertRuleConditionViewModel> DirectoryCondition { get; }

        public PsfProcess BaseObject { get; private set; }

        public string ProcessName
        {
            get => this.processName.CurrentValue;
            set
            {
                if (this.processName.CurrentValue == value)
                {
                    return;
                }

                this.processName.CurrentValue = value;
                this.OnPropertyChanged();
            }
        }

        public override string ToString()
        {
            string fixupname;

            switch (this.FixupMode)
            {
                case PsfExpertRuleConditionRedirectionMode.X64:
                    fixupname = "64-bit file redirection";
                    break;
                case PsfExpertRuleConditionRedirectionMode.X86:
                    fixupname = "32-bit file redirection";
                    break;
                default:
                    fixupname = this.FixupName;
                    break;
            }

            return "If process matches " + this.processName.CurrentValue + " with fixup " + fixupname;
        }
    }
}