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

using System.Collections.Generic;
using System.Linq;
using Otor.MsixHero.Appx.Reader.Psf.Entities.Descriptor;

namespace Otor.MsixHero.App.Modules.PackageManagement.PackageContent.ViewModel.Items.Psf
{
    public class PsfApplicationProxyViewModel : BaseApplicationProxyViewModel
    {
        private readonly PsfApplicationProxy _definition;
        
        public PsfApplicationProxyViewModel(string installFolder, PsfApplicationProxy definition) : base(definition)
        {
            this._definition = definition;

            if (this._definition.Tracing != null)
            {
                this.Tracing = new TracingPsfViewModel(this._definition.Tracing);
            }

            if (this._definition.Scripts != null)
            {
                this.Scripts = new List<AppxPsfScriptViewModel>(definition.Scripts.Select(s => new AppxPsfScriptViewModel(installFolder, s)));
            }
        }

        public List<PsfFolderRedirectionDescriptor> FileRedirections => this._definition.FileRedirections;
        
        public List<AppxPsfScriptViewModel> Scripts { get; }

        public List<string> OtherFixups => this._definition.OtherFixups;

        public bool IsAdvanced => this.HasArguments || this.HasTracing || this.HasWorkingDirectory || this.HasFileRedirections || this.HasOtherFixups;

        public TracingPsfViewModel Tracing { get; }

        public bool HasFileRedirections => this._definition.FileRedirections?.Count > 0;
        
        public bool HasScripts => this._definition.Scripts?.Count > 0;

        public bool HasOtherFixups => this._definition.OtherFixups?.Count > 0;

        public bool HasTracing => this.Tracing != null;
    }
}