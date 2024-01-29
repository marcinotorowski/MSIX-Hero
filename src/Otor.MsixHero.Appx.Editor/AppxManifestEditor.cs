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

using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Otor.MsixHero.Appx.Editor.Commands;

namespace Otor.MsixHero.Appx.Editor
{
    public class AppxManifestEditor : IAppxEditor
    {
        private readonly IAppxEditor extractedEditor;

        public AppxManifestEditor(FileInfo manifestPath)
        {
            this.extractedEditor = new AppxDirectoryEditor(manifestPath.Directory);
        }

        public AppxManifestEditor(string manifestPath)
        {
            this.extractedEditor = new AppxDirectoryEditor(Path.GetDirectoryName(manifestPath));
        }

        public Task Begin(CancellationToken cancellationToken = default)
        {
            return this.extractedEditor.Begin(cancellationToken);
        }

        public Task Finish(CancellationToken cancellationToken = default)
        {
            return this.extractedEditor.Finish(cancellationToken);
        }

        public Task Execute(IAppxEditCommand command, CancellationToken cancellationToken = default)
        {
            return this.extractedEditor.Execute(command, cancellationToken);
        }
    }
}