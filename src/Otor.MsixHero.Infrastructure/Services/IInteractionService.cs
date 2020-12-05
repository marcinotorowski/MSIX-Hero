using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Otor.MsixHero.Infrastructure.Services
{
    public interface IInteractionService
    {
        InteractionResult Confirm(string body, string title = null, InteractionType type = InteractionType.Information, InteractionButton buttons = InteractionButton.OK);

        bool SelectFile(string initialFile, string filterString, out string selectedFile);

        bool SaveFile(string initialFile, string filterString, out string selectedFile);

        bool SelectFile(string filterString, out string selectedFile);

        bool SaveFile(string filterString, out string selectedFile);

        bool SelectFiles(string initialFile, string filterString, out string[] selectedFiles);

        bool SelectFiles(string filterString, out string[] selectedFiles);

        bool SelectFile(out string selectedFile);

        bool SaveFile(out string selectedFile);

        bool SelectFolder(string initialFile, out string selectedFolder);

        bool SelectFolder(out string selectedFolder);

        InteractionResult ShowError(string body, InteractionResult buttons = InteractionResult.Close, string title = null, string extendedInfo = null);
        
        InteractionResult ShowInfo(string body, InteractionResult buttons = InteractionResult.Close, string title = null, string extendedInfo = null);

        InteractionResult ShowError(string body, Exception exception, InteractionResult buttons = InteractionResult.Close);

        int ShowMessage(string body, IReadOnlyCollection<string> buttons, string title = null, string extendedInfo = null, InteractionResult systemButtons = 0);

        Task ShowToast(string title, string message, InteractionType type = InteractionType.Information, Action clickCallback = null);
    }
}
