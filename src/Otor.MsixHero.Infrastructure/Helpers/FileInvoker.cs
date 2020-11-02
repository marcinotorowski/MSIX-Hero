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
