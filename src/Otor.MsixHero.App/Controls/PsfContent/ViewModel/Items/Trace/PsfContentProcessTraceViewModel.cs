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
