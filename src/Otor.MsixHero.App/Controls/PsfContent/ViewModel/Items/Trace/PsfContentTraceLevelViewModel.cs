using Otor.MsixHero.App.Changeable;
using Otor.MsixHero.Appx.Psf.Entities;

namespace Otor.MsixHero.App.Controls.PsfContent.ViewModel.Items.Trace
{
    public class PsfContentTraceLevelViewModel : ChangeableContainer
    {
        private readonly ChangeableProperty<TraceLevel> defaultLevel;
        private readonly ChangeableProperty<TraceLevel> filesystemLevel;
        private readonly ChangeableProperty<TraceLevel> registryLevel;
        private readonly ChangeableProperty<TraceLevel> processAndThreadLevel;
        private readonly ChangeableProperty<TraceLevel> dynamicLinkLibraryLevel;

        public PsfContentTraceLevelViewModel(PsfTraceFixupLevels levels)
        {
            this.defaultLevel = new ChangeableProperty<TraceLevel>(levels.Default);
            this.filesystemLevel = new ChangeableProperty<TraceLevel>(levels.Filesystem);
            this.registryLevel = new ChangeableProperty<TraceLevel>(levels.Registry);
            this.processAndThreadLevel = new ChangeableProperty<TraceLevel>(levels.ProcessAndThread);
            this.dynamicLinkLibraryLevel = new ChangeableProperty<TraceLevel>(levels.DynamicLinkLibrary);

            this.AddChildren(this.defaultLevel, this.filesystemLevel, this.registryLevel, this.processAndThreadLevel, this.dynamicLinkLibraryLevel);
        }

        public TraceLevel DefaultLevel
        {
            get => this.defaultLevel.CurrentValue;
            set
            {
                if (this.defaultLevel.CurrentValue == value)
                {
                    return;
                }

                this.defaultLevel.CurrentValue = value;
                this.OnPropertyChanged();
            }
        }

        public TraceLevel FilesystemLevel
        {
            get => this.filesystemLevel.CurrentValue;
            set
            {
                if (this.filesystemLevel.CurrentValue == value)
                {
                    return;
                }

                this.filesystemLevel.CurrentValue = value;
                this.OnPropertyChanged();
            }
        }

        public TraceLevel RegistryLevel
        {
            get => this.registryLevel.CurrentValue;
            set
            {
                if (this.registryLevel.CurrentValue == value)
                {
                    return;
                }

                this.registryLevel.CurrentValue = value;
                this.OnPropertyChanged();
            }
        }

        public TraceLevel ProcessAndThreadLevel
        {
            get => this.processAndThreadLevel.CurrentValue;
            set
            {
                if (this.processAndThreadLevel.CurrentValue == value)
                {
                    return;
                }

                this.processAndThreadLevel.CurrentValue = value;
                this.OnPropertyChanged();
            }
        }

        public TraceLevel DynamicLinkLibraryLevel
        {
            get => this.dynamicLinkLibraryLevel.CurrentValue;
            set
            {
                if (this.dynamicLinkLibraryLevel.CurrentValue == value)
                {
                    return;
                }

                this.dynamicLinkLibraryLevel.CurrentValue = value;
                this.OnPropertyChanged();
            }
        }
    }
}
