using System.Collections.Generic;
using otor.msixhero.ui.Domain;

namespace otor.msixhero.ui.Modules.Common.PsfContent.ViewModel.Items
{
    public class PsfContentRuleViewModel : ChangeableContainer
    {
        private readonly ChangeableProperty<string> targetRedirection;
        private readonly ChangeableProperty<bool> isReadOnly;

        public PsfContentRuleViewModel(IEnumerable<string> include, IEnumerable<string> exclude, string redirectBase, bool isReadOnly)
        {
            this.targetRedirection = new ChangeableProperty<string>(redirectBase);
            this.isReadOnly = new ChangeableProperty<bool>(isReadOnly);

            this.Include = new ChangeableCollection<PsfContentFileViewModel>();
            this.Exclude = new ChangeableCollection<PsfContentFileViewModel>();

            foreach (var item in include)
            {
                this.Include.Add(new PsfContentFileViewModel(item));
            }

            foreach (var item in exclude)
            {
                this.Exclude.Add(new PsfContentFileViewModel(item, true));
            }

            this.Include.Commit();
            this.Exclude.Commit();

            this.AddChildren(this.Include, this.Exclude, this.isReadOnly, this.targetRedirection);
        }

        public PsfContentRuleViewModel(IEnumerable<PsfContentFileViewModel> include, IEnumerable<PsfContentFileViewModel> exclude, string redirectBase, bool isReadOnly)
        {
            this.targetRedirection = new ChangeableProperty<string>(redirectBase);
            this.isReadOnly = new ChangeableProperty<bool>(isReadOnly);

            this.Include = new ChangeableCollection<PsfContentFileViewModel>(include);
            this.Exclude = new ChangeableCollection<PsfContentFileViewModel>(exclude);

            this.Include.Commit();
            this.Exclude.Commit();

            this.AddChildren(this.Include, this.Exclude, this.isReadOnly, this.targetRedirection);
        }

        public ChangeableCollection<PsfContentFileViewModel> Include { get; }

        public ChangeableCollection<PsfContentFileViewModel> Exclude { get; }

        public string TargetRedirection
        {
            get => this.targetRedirection.CurrentValue;
            set
            {
                if (this.targetRedirection.CurrentValue == value)
                {
                    return;
                }

                this.targetRedirection.CurrentValue = value;
                this.OnPropertyChanged();
            }
        }

        public bool IsReadOnly
        {
            get => this.isReadOnly.CurrentValue;
            set
            {
                if (this.isReadOnly.CurrentValue == value)
                {
                    return;
                }

                this.isReadOnly.CurrentValue = value;
                this.OnPropertyChanged();
            }
        }
    }
}
