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

using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using Otor.MsixHero.App.Mvvm;
using Otor.MsixHero.Appx.Psf.Entities.Descriptor;

namespace Otor.MsixHero.App.Controls.PackageExpert.ViewModels.Items.Psf
{
    public class AppxPsfViewModel : NotifyPropertyChanged
    {
        private readonly PsfApplicationDescriptor definition;

        public static readonly ICommand EditScript = new RoutedUICommand() { Text = "Edit script" };

        public AppxPsfViewModel(string installFolder, PsfApplicationDescriptor definition)
        {
            this.definition = definition;

            if (this.definition.Tracing != null)
            {
                this.Tracing = new TracingPsfViewModel(this.definition.Tracing);
            }

            if (this.definition.Scripts != null)
            {
                this.Scripts = new List<AppxPsfScriptViewModel>(definition.Scripts.Select(s => new AppxPsfScriptViewModel(installFolder, s)));
            }
        }

        public string Executable => this.definition.Executable;
        
        public string Arguments => this.definition.Arguments;
        
        public string WorkingDirectory => this.definition.WorkingDirectory;
        
        public List<PsfFolderRedirectionDescriptor> FileRedirections => this.definition.FileRedirections;
        
        public List<AppxPsfScriptViewModel> Scripts { get; }

        public List<string> OtherFixups => this.definition.OtherFixups;

        public bool HasArguments => !string.IsNullOrEmpty(this.definition.Arguments);

        public bool IsAdvanced => this.HasArguments || this.HasTracing || this.HasWorkingDirectory || this.HasFileRedirections || this.HasOtherFixups;

        public bool HasWorkingDirectory => !string.IsNullOrEmpty(this.definition.WorkingDirectory);

        public TracingPsfViewModel Tracing { get; }

        public bool HasFileRedirections => this.definition.FileRedirections?.Count > 0;
        
        public bool HasScripts => this.definition.Scripts?.Count > 0;

        public bool HasOtherFixups => this.definition.OtherFixups?.Count > 0;

        public bool HasTracing => this.Tracing != null;
    }
}