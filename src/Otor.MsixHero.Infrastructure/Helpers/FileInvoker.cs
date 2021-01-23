// MSIX Hero
// Copyright (C) 2021 Marcin Otorowski
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
using Otor.MsixHero.Infrastructure.Configuration;
using Otor.MsixHero.Infrastructure.Configuration.ResolvableFolder;
using Otor.MsixHero.Infrastructure.Logging;
using Otor.MsixHero.Infrastructure.Services;

namespace Otor.MsixHero.Infrastructure.Helpers
{
    public class FileInvoker
    {
        private static readonly ILog Logger = LogManager.GetLogger<FileInvoker>();

        private readonly IInteractionService interactionService;

        public FileInvoker(IInteractionService interactionService)
        {
            this.interactionService = interactionService;
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
                Logger.Error(e, $"Could not open the file {filePath.Resolved}");

                if (throwExceptions)
                {
                    throw;
                }

                this.interactionService.ShowError($"Could not open the file {filePath.Resolved}", e, InteractionResult.OK);
            }
        }
    }
}
