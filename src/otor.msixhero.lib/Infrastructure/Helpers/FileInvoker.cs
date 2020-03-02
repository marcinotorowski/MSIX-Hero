using System;
using System.Diagnostics;
using NLog.Fluent;
using otor.msixhero.lib.Infrastructure.Configuration;
using otor.msixhero.lib.Infrastructure.Configuration.ResolvableFolder;
using otor.msixhero.lib.Infrastructure.Logging;

namespace otor.msixhero.lib.Infrastructure.Helpers
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
