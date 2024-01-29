// MSIX Hero
// Copyright (C) 2024 Marcin Otorowski
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

namespace Otor.MsixHero.App.Modules.PackageManagement.PackageContent.ViewModel.Psf.Items
{
    public class ScriptViewModel : NotifyPropertyChanged
    {
        private readonly PsfScriptDescriptor _descriptor;

        public ScriptViewModel(string applicationName, string parentFolder, PsfScriptDescriptor descriptor)
        {
            this._descriptor = descriptor;
            this.FullLocalPath = System.IO.Path.Combine(parentFolder, descriptor.Name);
            this.ParentApplicationName = applicationName;
        }

        public string Content { get; set; }

        public string ParentApplicationName { get; }

        public string Name => _descriptor.Name;

        public string Arguments => _descriptor.Arguments;

        public PsfScriptDescriptorTiming Timing => _descriptor.Timing;

        public bool HasArguments => !string.IsNullOrEmpty(Arguments);

        public bool InVirtualEnvironment => _descriptor.RunInVirtualEnvironment;

        public bool WaitForFinish => _descriptor.WaitForScriptToFinish;

        public bool ShowWindow => _descriptor.ShowWindow;

        public bool OnlyOnce => _descriptor.RunOnce;

        public string FullLocalPath { get; }
    }
}
