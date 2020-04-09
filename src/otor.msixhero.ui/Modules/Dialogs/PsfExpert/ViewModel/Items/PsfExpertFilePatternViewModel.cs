using System.Text.RegularExpressions;
using otor.msixhero.ui.Domain;
using otor.msixhero.ui.ViewModel;

namespace otor.msixhero.ui.Modules.Dialogs.PsfExpert.ViewModel.Items
{
    public class PsfExpertFilePatternViewModel : ChangeableContainer
    {
        private readonly ChangeableProperty<RedirectionType> redirectionType;
        private readonly ChangeableProperty<string> basePath;
        private readonly ChangeableProperty<string> relativePath;
        private readonly ChangeableProperty<string> pattern;
        private readonly ChangeableProperty<bool> isReadOnly;
        private readonly ChangeableProperty<string> redirectTargetBase;
        private bool isFirst;

        public PsfExpertFilePatternViewModel(RedirectionType redirectionType, bool isExclusion, string basePath, string relativePath, string pattern, bool isReadOnly, string redirectTargetBase)
        {
            this.IsExclusion = isExclusion;
            this.pattern = new ChangeableProperty<string>(pattern);
            this.relativePath = new ChangeableProperty<string>(relativePath);
            this.basePath = new ChangeableProperty<string>(basePath);
            this.redirectionType = new ChangeableProperty<RedirectionType>(redirectionType);
            this.isReadOnly = new ChangeableProperty<bool>(isReadOnly);
            this.redirectTargetBase = new ChangeableProperty<string>(redirectTargetBase);
            this.RegexMode = new PsfExpertRegexMode(pattern, isExclusion);

            this.AddChildren(this.pattern, this.basePath, this.relativePath, this.redirectionType, this.isReadOnly, this.redirectTargetBase);
        }

        public PsfExpertFilePatternViewModel(RedirectionType redirectionType, bool isExclusion, string relativePath, string pattern, bool isReadOnly, string redirectTargetBase)
        {
            this.IsExclusion = isExclusion;
            this.pattern = new ChangeableProperty<string>(pattern);
            this.relativePath = new ChangeableProperty<string>(relativePath);
            this.basePath = new ChangeableProperty<string>(null);
            this.redirectionType = new ChangeableProperty<RedirectionType>(redirectionType);
            this.isReadOnly = new ChangeableProperty<bool>(isReadOnly);
            this.redirectTargetBase = new ChangeableProperty<string>(redirectTargetBase);
            this.RegexMode = new PsfExpertRegexMode(pattern, isExclusion);

            this.AddChildren(this.pattern, this.basePath, this.relativePath, this.redirectionType, this.isReadOnly, this.redirectTargetBase);
        }

        public bool IsFirst
        {
            get => isFirst;
            set => this.SetField(ref this.isFirst, value);
        }

        public bool IsExclusion { get; }

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

        public PsfExpertRegexMode RegexMode { get; }

        public string RedirectTargetBase
        {
            get => this.redirectTargetBase.CurrentValue;
            set
            {
                if (this.redirectTargetBase.CurrentValue == value)
                {
                    return;
                }

                this.redirectTargetBase.CurrentValue = value;
                this.OnPropertyChanged();
            }
        }

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

    public class PsfExpertRegexMode : NotifyPropertyChanged
    {
        private readonly bool isExclusion;

        public PsfExpertRegexMode(string regex, bool isExclusion)
        {
            this.isExclusion = isExclusion;
            this.SetFromRegex(regex);
        }

        public string Prompt { get; private set; }

        public string DisplayName { get; private set; }
        
        public void SetFromRegex(string regex)
        {
            string newPrompt = null;
            string newDisplayName = null;

            if (regex == ".*")
            {
                newPrompt = this.isExclusion ? "is not" : "file name is";
                newDisplayName = "(any)";
            }
            else if (regex == @".*\..*")
            {
                newPrompt = this.isExclusion ? "is not" : "file name is";
                newDisplayName = "(any with extension)";
            }
            else if (Regex.IsMatch(regex, @"^\^[a-zA-Z0-9_]$"))
            {
                newPrompt = this.isExclusion ? "does not start with" : "starts with";
                newDisplayName = regex.Substring(1);
            }
            else if (Regex.IsMatch(regex, @"^[a-zA-Z0-9_]\$$"))
            {
                newPrompt = this.isExclusion ? "does not end with" : "ends with";
                newDisplayName = regex.Substring(0, regex.Length - 1);
            }
            else
            {
                var match = Regex.Match(regex, @"^\^?(\w+)\\.(\w+)\$?$");
                if (match.Success)
                {
                    newPrompt = this.isExclusion ? "is not" : "file name is";
                    newDisplayName = match.Groups[1].Value + "." + match.Groups[2].Value;
                }
                else
                {
                    match = Regex.Match(regex, @"^\^?\.\*\\\.\[(\w{2})\]\[(\w{2})\]\[(\w{2})\]\$$");
                    if (match.Success)
                    {
                        var f1 = match.Groups[1].Value.ToLowerInvariant();
                        var f2 = match.Groups[2].Value.ToLowerInvariant();
                        var f3 = match.Groups[3].Value.ToLowerInvariant();

                        if (f1[0] == f1[1] && f2[0] == f2[1] && f3[0] == f3[1])
                        {
                            newPrompt = this.isExclusion ? "has file extension other than" : "file extension is";
                            newDisplayName = "*." + f1[0] + f2[0] + f3[0];
                        }
                    }
                    else
                    {

                        match = Regex.Match(regex, @"^\^?\.*\\.([a-z0-9]+)");
                        if (match.Success)
                        {
                            newPrompt = this.isExclusion ? "has file extension other than" : "file extension is";
                            newDisplayName = "*." + match.Groups[1].Value;
                        }
                    }
                }
            }

            if (newDisplayName != null && newPrompt != null)
            {
                this.Prompt = newPrompt;
                this.DisplayName = newDisplayName;
            }
            else if (this.isExclusion)
            {
                this.Prompt = "does not match";
                this.DisplayName = regex;
            }
            else
            {
                this.Prompt = "file name matches";
                this.DisplayName = regex;
            }

            this.OnPropertyChanged(nameof(this.Prompt));
            this.OnPropertyChanged(nameof(this.DisplayName));
        }
    }
}