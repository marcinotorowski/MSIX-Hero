namespace otor.msixhero.lib.Services
{
    public enum InteractionResult
    {
        Cancel	= 2,
        No = 7,
        None = 0,
        OK = 1,
        Yes = 6	,
        Retry = 4,
        Close = 8
    }

    public enum InteractionButton
    {
        OK = 0,
        OKCancel = 1,
        YesNo = 4,
        YesNoCancel = 3
    }

    public enum InteractionType
    {
        Asterisk = 64,
        Error = 16,
        Exclamation = 48,
        Hand = 16,
        Information = 64,
        None = 0,
        Question = 32,
        Stop = 16,
        Warning	= 48,
    }


    public interface IInteractionService
    {
        InteractionResult Confirm(string body, string title = null, InteractionType type = InteractionType.Information, InteractionButton buttons = InteractionButton.OK);

        bool SelectFile(string initialFile, string filterString, out string selectedFile);

        bool SelectFile(string filterString, out string selectedFile);

        bool SelectFile(out string selectedFile);
        
        bool SelectFolder(string initialFile, out string selectedFolder);

        bool SelectFolder(out string selectedFolder);

        InteractionResult ShowError(string body, string title = null, string extendedInfo = null);
    }
}
