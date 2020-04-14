using System.Collections.Generic;
using System.Linq;
using otor.msixhero.ui.Domain;

namespace otor.msixhero.ui.Modules.Common.PsfContent.ViewModel.Items
{
    public enum PsfContentFolderRelationTo
    {
        PackageRoot,
        Drive,
        KnownFolder
    }

    public class PsfContentFolderViewModel : ChangeableContainer
    {
        private readonly ChangeableProperty<string> knownDir;

        private readonly ChangeableProperty<string> baseDir;
        
        private readonly ChangeableProperty<PsfContentFolderRelationTo> relationTo;

        public PsfContentFolderViewModel(PsfContentFolderRelationTo relation, string baseDir, IEnumerable<PsfContentRuleViewModel> rules) : this(relation, null, baseDir, rules)
        {
        }

        public PsfContentFolderViewModel(string knownDir, string baseDir, IEnumerable<PsfContentRuleViewModel> rules) : this(PsfContentFolderRelationTo.KnownFolder, knownDir, baseDir, rules)
        {
        }

        private PsfContentFolderViewModel(PsfContentFolderRelationTo relation, string knownDir, string baseDir, IEnumerable<PsfContentRuleViewModel> rules)
        {
            this.knownDir = new ChangeableProperty<string>(knownDir);
            this.baseDir = new ChangeableProperty<string>(baseDir);
            this.relationTo = new ChangeableProperty<PsfContentFolderRelationTo>(relation);
            
            this.Rules = new ChangeableCollection<PsfContentRuleViewModel>(rules.OrderBy(r => r.Exclude.Any() ? 1 : 0));
            this.Rules.Commit();
            this.AddChildren(this.Rules, this.knownDir, this.relationTo, this.baseDir);

            this.knownDir.ValueChanged += this.DirectoryOnValueChanged;
            this.baseDir.ValueChanged += this.DirectoryOnValueChanged;
            this.relationTo.ValueChanged += this.DirectoryOnValueChanged;
        }

        public string FullDir
        {
            get
            {
                switch (this.RelationTo)
                {
                    case PsfContentFolderRelationTo.KnownFolder when string.IsNullOrEmpty(this.BaseDir):
                        return $@"{{{this.KnownDir}}}\";
                    case PsfContentFolderRelationTo.KnownFolder:
                        return $@"{{{this.KnownDir}}}\{this.BaseDir}\";
                    case PsfContentFolderRelationTo.PackageRoot when string.IsNullOrEmpty(this.BaseDir):
                        return "(Package Directory)";
                    case PsfContentFolderRelationTo.PackageRoot:
                        return $@"(Package Directory)\{this.BaseDir}\";
                    case PsfContentFolderRelationTo.Drive when string.IsNullOrEmpty(this.BaseDir):
                        return "(Root Drive)";
                    case PsfContentFolderRelationTo.Drive:
                        return $@"(Root Drive)\{this.BaseDir}\";
                    default:
                        return this.BaseDir;
                }
            }
        }

        public PsfContentFolderRelationTo RelationTo
        {
            get => this.relationTo.CurrentValue;

            set
            {
                if (this.relationTo.CurrentValue == value)
                {
                    return;
                }

                this.relationTo.CurrentValue = value;
                this.OnPropertyChanged();
            }
        }

        public string BaseDir
        {
            get => this.baseDir.CurrentValue;

            set
            {
                if (this.baseDir.CurrentValue == value)
                {
                    return;
                }

                this.baseDir.CurrentValue = value;
                this.OnPropertyChanged();
            }
        }

        public string KnownDir
        {
            get => this.knownDir.CurrentValue;

            set
            {
                if (this.knownDir.CurrentValue == value)
                {
                    return;
                }

                this.knownDir.CurrentValue = value;
                this.OnPropertyChanged();
            }
        }

        public ChangeableCollection<PsfContentRuleViewModel> Rules { get; }

        private void DirectoryOnValueChanged(object sender, ValueChangedEventArgs e)
        {
            this.OnPropertyChanged(nameof(this.FullDir));
        }
    }
}
