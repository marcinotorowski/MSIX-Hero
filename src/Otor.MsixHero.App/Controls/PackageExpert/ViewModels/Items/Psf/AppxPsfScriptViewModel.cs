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

using Otor.MsixHero.App.Mvvm;
using Otor.MsixHero.Appx.Psf.Entities.Descriptor;

namespace Otor.MsixHero.App.Controls.PackageExpert.ViewModels.Items.Psf
{
    public class AppxPsfScriptViewModel : NotifyPropertyChanged
    {
        private readonly PsfScriptDescriptor descriptor;

        public AppxPsfScriptViewModel(string parentFolder, PsfScriptDescriptor descriptor)
        {
            this.descriptor = descriptor;
            this.FullLocalPath = System.IO.Path.Combine(parentFolder, descriptor.Name);
        }

        public string Name
        {
            get => this.descriptor.Name;
        }

        public string Arguments
        {
            get => this.descriptor.Arguments;
        }

        public PsfScriptDescriptorTiming Timing
        {
            get => this.descriptor.Timing;
        }

        public bool HasArguments
        {
            get => !string.IsNullOrEmpty(this.Arguments);
        }

        public bool InVirtualEnvironment
        {
            get => this.descriptor.RunInVirtualEnvironment;
        }

        public bool WaitForFinish
        {
            get => this.descriptor.WaitForScriptToFinish;
        }

        public bool ShowWindow
        {
            get => this.descriptor.ShowWindow;
        }

        public bool OnlyOnce
        {
            get => this.descriptor.RunOnce;
        }

        public string FullLocalPath { get; }
    }
}
