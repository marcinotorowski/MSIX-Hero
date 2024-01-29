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

using System;
using System.IO;
using System.Windows.Input;
using Otor.MsixHero.Infrastructure.Services;
using Prism.Commands;

namespace Otor.MsixHero.App.Mvvm.Changeable
{
    public class ChangeableFolderProperty : ValidatedChangeableProperty<string>
    {
        // ReSharper disable once InconsistentNaming
        private static Func<string, string> validatePath;

        // ReSharper disable once InconsistentNaming
        private static Func<string, string> validatePathAndPresence;

        private readonly IInteractionService interactionService;
        private ICommand browse;

        public ChangeableFolderProperty(string displayName, IInteractionService interactionService, string initialFolder) : base(displayName, initialFolder)
        {
            this.interactionService = interactionService;
        }

        public ChangeableFolderProperty(string displayName, IInteractionService interactionService, string initialFolder, params Func<string, string>[] validators) : base(displayName, initialFolder, validators)
        {
            this.interactionService = interactionService;
        }

        public ChangeableFolderProperty(string displayName, IInteractionService interactionService, params Func<string, string>[] validators) : base(displayName, validators)
        {
            this.interactionService = interactionService;
        }

        public ChangeableFolderProperty(Func<string> displayName, IInteractionService interactionService, string initialFolder) : base(displayName, initialFolder)
        {
            this.interactionService = interactionService;
        }

        public ChangeableFolderProperty(Func<string> displayName, IInteractionService interactionService, string initialFolder, params Func<string, string>[] validators) : base(displayName, initialFolder, validators)
        {
            this.interactionService = interactionService;
        }

        public ChangeableFolderProperty(Func<string> displayName, IInteractionService interactionService, params Func<string, string>[] validators) : base(displayName, validators)
        {
            this.interactionService = interactionService;
        }

        public static Func<string, string> ValidatePath
        {
            get
            {
                return validatePath ??= value =>
                {
                    if (string.IsNullOrEmpty(value))
                    {
                        return Resources.Localization.Dialogs_FolderPicker_Validation_Empty;
                    }

                    if (!Uri.TryCreate(value, UriKind.Absolute, out _))
                    {
                        return Resources.Localization.Dialogs_FolderPicker_Validation_Invalid;
                    }
                    
                    return null;
                };
            }
        }

        public static Func<string, string> ValidatePathAndPresence
        {
            get
            {
                return validatePathAndPresence ??= value =>
                {
                    if (string.IsNullOrEmpty(value))
                    {
                        return Resources.Localization.Dialogs_FolderPicker_Validation_Empty;
                    }

                    if (!Uri.TryCreate(value, UriKind.Absolute, out _))
                    {
                        return Resources.Localization.Dialogs_FolderPicker_Validation_Invalid;
                    }


                    if (!Directory.Exists(value))
                    {
                        return Resources.Localization.Dialogs_FolderPicker_Validation_Missing;
                    }

                    return null;
                };
            }
        }

        public ICommand Browse
        {
            get
            {
                return this.browse ??= new DelegateCommand(() =>
                {
                    if (string.IsNullOrEmpty(this.CurrentValue))
                    {
                        if (this.interactionService.SelectFolder(out var newValue))
                        {
                            this.CurrentValue = newValue;
                        }
                    }
                    else
                    {
                        if (this.interactionService.SelectFolder(this.CurrentValue, out var newValue))
                        {
                            this.CurrentValue = newValue;
                        }
                    }
                });
            }
        }
    }
}