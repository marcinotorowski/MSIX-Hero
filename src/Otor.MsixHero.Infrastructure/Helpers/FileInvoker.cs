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

using System;
using System.Diagnostics;
using System.IO;
using Otor.MsixHero.Infrastructure.Configuration;
using Otor.MsixHero.Infrastructure.Configuration.ResolvableFolder;
using Dapplo.Log;
using Otor.MsixHero.Infrastructure.Services;

namespace Otor.MsixHero.Infrastructure.Helpers
{
    public class FileInvoker
    {
        private static readonly LogSource Logger = new();
        private readonly IInteractionService interactionService;
        private readonly IConfigurationService configurationService;

        public FileInvoker(IInteractionService interactionService, IConfigurationService configurationService)
        {
            this.interactionService = interactionService;
            this.configurationService = configurationService;
        }

        public void Execute(ResolvablePath filePath, bool throwExceptions = false)
        {
            ResolvablePath toolPath = null;
            EditorType? editorType = null;
            
            var config = this.configurationService.GetCurrentConfiguration().Editing;

            if (string.Equals("appxmanifest.xml", Path.GetFileName(filePath.Resolved), StringComparison.OrdinalIgnoreCase))
            {
                toolPath = config?.ManifestEditor;
                editorType = config?.ManifestEditorType;
            }
            else if (string.Equals("config.json", Path.GetFileName(filePath.Resolved), StringComparison.OrdinalIgnoreCase))
            {
                toolPath = config?.PsfEditor;
                editorType = config?.PsfEditorType;
            }
            else
            {
                var ext = Path.GetExtension(filePath).ToLowerInvariant();

                switch (ext)
                {
                    case ".msix":
                    case ".appx":
                        toolPath = config?.MsixEditor;
                        editorType = config?.MsixEditorType;
                        break;
                    case ".appinstaller":
                        toolPath = config?.AppInstallerEditor;
                        editorType = config?.AppInstallerEditorType;
                        break;
                    case ".ps1":
                        toolPath = config?.PowerShellEditor;
                        editorType = config?.PowerShellEditorType;
                        break;
                }
            }

            if (editorType.HasValue)
            {
                this.Execute(editorType.Value, toolPath, filePath, throwExceptions);
            }
            else
            {
                this.Execute(EditorType.Default, null, filePath, throwExceptions);
            }
        }
        
        public void Execute(EditorType editorType, ResolvablePath toolPath, ResolvablePath filePath, bool throwExceptions = false)
        {
            try
            {
                switch (editorType)
                {
                    case EditorType.Default:
                    {
                        var spi = new ProcessStartInfo(filePath.Resolved) { UseShellExecute = true };
                        Process.Start(spi);
                        break;
                    }

                    case EditorType.Ask:
                    {
                        Process.Start("rundll32.exe", "shell32.dll, OpenAs_RunDLL " + filePath.Resolved);
                        break;
                    }

                    default:
                    {
                        var spi = new ProcessStartInfo(toolPath.Resolved, "\"" + filePath.Resolved + "\"") { UseShellExecute = true };
                        Process.Start(spi);
                        break;
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Error().WriteLine($"Could not open the file {filePath.Resolved}");
                Logger.Error().WriteLine(e);

                if (throwExceptions)
                {
                    throw;
                }

                this.interactionService.ShowError($"Could not open the file {filePath.Resolved}", e, InteractionResult.OK);
            }
        }
    }
}
