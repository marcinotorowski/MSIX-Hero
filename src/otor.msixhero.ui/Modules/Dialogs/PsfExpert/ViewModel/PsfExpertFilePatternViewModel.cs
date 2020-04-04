using System;
using otor.msixhero.ui.Domain;

namespace otor.msixhero.ui.Modules.Dialogs.PsfExpert.ViewModel
{
    public class PsfExpertFilePatternViewModel : ChangeableContainer
    {
        private readonly ChangeableProperty<RedirectionType> redirectionType;
        private readonly ChangeableProperty<string> basePath;
        private readonly ChangeableProperty<string> relativePath;
        private readonly ChangeableProperty<string> pattern;
        private bool isFirst;

        public PsfExpertFilePatternViewModel(RedirectionType redirectionType, bool isExclusion, string basePath, string relativePath, string pattern)
        {
            this.IsExclusion = isExclusion;
            this.pattern = new ChangeableProperty<string>(pattern);
            this.relativePath = new ChangeableProperty<string>(relativePath);
            this.basePath = new ChangeableProperty<string>(basePath);
            this.redirectionType = new ChangeableProperty<RedirectionType>(redirectionType);
            this.AddChildren(this.pattern, this.basePath, this.relativePath, this.redirectionType);
        }

        public PsfExpertFilePatternViewModel(RedirectionType redirectionType, bool isExclusion, string relativePath, string pattern)
        {
            this.IsExclusion = isExclusion;
            this.pattern = new ChangeableProperty<string>(pattern);
            this.relativePath = new ChangeableProperty<string>(relativePath);
            this.basePath = new ChangeableProperty<string>(null);
            this.redirectionType = new ChangeableProperty<RedirectionType>(redirectionType);
            this.AddChildren(this.pattern, this.basePath, this.relativePath, this.redirectionType);
        }

        public bool IsFirst
        {
            get => isFirst;
            set => this.SetField(ref this.isFirst, value);
        }
        public bool IsExclusion { get; }

        public RedirectionType RedirectionType
        {
            get => this.redirectionType.CurrentValue;
            set
            {
                if (this.redirectionType.CurrentValue == value)
                {
                    return;
                }

                this.redirectionType.CurrentValue = value;
                this.OnPropertyChanged();
            }
        }

        public string BasePath
        {
            get => this.basePath.CurrentValue;
            set
            {
                if (this.basePath.CurrentValue == value)
                {
                    return;
                }

                this.basePath.CurrentValue = value;
                this.OnPropertyChanged();
            }
        }

        public string RelativePath
        {
            get => this.relativePath.CurrentValue;
            set
            {
                if (this.relativePath.CurrentValue == value)
                {
                    return;
                }

                this.relativePath.CurrentValue = value;
                this.OnPropertyChanged();
            }
        }

        public string Pattern
        {
            get => this.pattern.CurrentValue;
            set
            {
                if (this.pattern.CurrentValue == value)
                {
                    return;
                }

                this.pattern.CurrentValue = value;
                this.OnPropertyChanged();
            }
        }

        public string FullPath
        {
            get
            {
                string token;
                switch (this.RedirectionType)
                {
                    case RedirectionType.PackageRelative:
                        token = "<PackageDirectory>\\" + this.RelativePath;
                        break;
                    case RedirectionType.RootRelative:
                        token = "<RootDrive>\\" + this.RelativePath;
                        break;
                    case RedirectionType.KnownFolder:
                        token = "{" + this.BasePath + "}" + this.RelativePath;
                        break;
                    default:
                        token = "<UNSUPPORTED>";
                        break;
                }

                return token;
            }
        }

        public override string ToString()
        {
            if (this.IsExclusion)
            {
                return "accessed path in " + this.FullPath + " does not match " + this.Pattern;
            }

            if (this.isFirst)
            {
                return "accessed path in " + this.FullPath + " matches " + this.Pattern;
            }

            return "OR accessed path in " + this.FullPath + " matches " + this.Pattern;
        }
    }
}