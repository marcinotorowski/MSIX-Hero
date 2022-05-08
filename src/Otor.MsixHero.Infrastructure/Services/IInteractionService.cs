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
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Otor.MsixHero.Infrastructure.Services
{
    public interface IInteractionService
    {
        InteractionResult Confirm(string body, string title = null, InteractionType type = InteractionType.Information, InteractionButton buttons = InteractionButton.OK);

        bool SelectFile(out string selectedFile);
        
        bool SelectFile(FileDialogSettings settings, out string selectedFile);
        
        bool SaveFile(out string selectedFile);
        
        bool SaveFile(FileDialogSettings settings, out string selectedFile);

        bool SelectFiles(out string[] selectedFiles);
        
        bool SelectFiles(FileDialogSettings settings, out string[] selectedFiles);
        
        bool SelectFolder(string initialFile, out string selectedFolder);

        bool SelectFolder(out string selectedFolder);

        InteractionResult ShowError(string body, InteractionResult buttons = InteractionResult.Close, string title = null, string extendedInfo = null);
        
        InteractionResult ShowInfo(string body, InteractionResult buttons = InteractionResult.Close, string title = null, string extendedInfo = null);

        InteractionResult ShowError(string body, Exception exception, InteractionResult buttons = InteractionResult.Close);

        int ShowMessage(string body, IReadOnlyCollection<string> buttons, string title = null, string extendedInfo = null, InteractionResult systemButtons = 0);

        Task ShowToast(string title, string message);
    }
}
