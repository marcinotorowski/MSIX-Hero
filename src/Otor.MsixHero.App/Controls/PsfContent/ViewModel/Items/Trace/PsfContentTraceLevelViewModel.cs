// MSIX Hero
// Copyright (C) 2022 Marcin Otorowski
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// Full notice:
// https://github.com/marcinotorowski/msix-hero/blob/develop/LICENSE.md

using Otor.MsixHero.App.Mvvm.Changeable;
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
