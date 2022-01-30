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
using System.IO;
using System.Windows.Input;
using Otor.MsixHero.Infrastructure.Services;
using Prism.Commands;

namespace Otor.MsixHero.App.Mvvm.Changeable
{
    public class ChangeableFileProperty : ValidatedChangeableProperty<string>
    {
        // ReSharper disable once InconsistentNaming
        private static Func<string, string> validatePath;
        
        // ReSharper disable once InconsistentNaming
        private static Func<string, string> validatePathAndPresence;

        private readonly IInteractionService interactionService;
        private ICommand browse;

        public ChangeableFileProperty(string displayName, IInteractionService interactionService, string initialFile) : base(displayName, initialFile)
        {
            this.interactionService = interactionService;
        }

        public ChangeableFileProperty(string displayName, IInteractionService interactionService, string initialFile, params Func<string, string>[] validators) : base(displayName, initialFile, validators)
        {
            this.interactionService = interactionService;
        }

        public ChangeableFileProperty(string displayName, IInteractionService interactionService, params Func<string, string>[] validators) : base(displayName, validators)
        {
            this.interactionService = interactionService;
        }

        public ChangeableFileProperty(Func<string> displayNamePredicate, IInteractionService interactionService, string initialFile) : base(displayNamePredicate, initialFile)
        {
            this.interactionService = interactionService;
        }

        public ChangeableFileProperty(Func<string> displayNamePredicate, IInteractionService interactionService, string initialFile, params Func<string, string>[] validators) : base(displayNamePredicate, initialFile, validators)
        {
            this.interactionService = interactionService;
        }

        public ChangeableFileProperty(Func<string> displayNamePredicate, IInteractionService interactionService, params Func<string, string>[] validators) : base(displayNamePredicate, validators)
        {
            this.interactionService = interactionService;
        }

        public string Filter { get; set; }

        public bool OpenForSaving { get; set; }

        public string Prompt { get; set; }
        
        public static Func<string, string> ValidatePath
        {
            get
            {
                return validatePath ??= value =>
                {
                    if (string.IsNullOrEmpty(value))
                    {
                        return Resources.Localization.Dialogs_FilePicker_Validation_Empty;
                    }

                    if (!Uri.TryCreate(value, UriKind.Absolute, out _))
                    {
                        return Resources.Localization.Dialogs_FilePicker_Validation_Invalid;
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
                        return Resources.Localization.Dialogs_FilePicker_Validation_Empty;
                    }

                    if (!Uri.TryCreate(value, UriKind.Absolute, out _))
                    {
                        return Resources.Localization.Dialogs_FilePicker_Validation_Invalid;
                    }


                    if (!File.Exists(value))
                    {
                        return Resources.Localization.Dialogs_FilePicker_Validation_Missing;
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
                    if (this.OpenForSaving)
                    {
                        if (string.IsNullOrEmpty(this.CurrentValue))
                        {
                            var settings = new FileDialogSettings(this.Filter, this.Prompt);
                            if (this.interactionService.SaveFile(settings, out var newValue))
                            {
                                this.CurrentValue = newValue;
                            }
                        }
                        else
                        {
                            var settings = new FileDialogSettings(this.Filter, this.Prompt, this.CurrentValue);
                            if (this.interactionService.SaveFile(settings, out var newValue))
                            {
                                this.CurrentValue = newValue;
                            }
                        }
                    }
                    else
                    {
                        if (string.IsNullOrEmpty(this.CurrentValue))
                        {
                            var settings = new FileDialogSettings(this.Filter, this.Prompt);
                            if (this.interactionService.SelectFile(settings, out var newValue))
                            {
                                this.CurrentValue = newValue;
                            }
                        }
                        else
                        {
                            var settings = new FileDialogSettings(this.Filter, this.Prompt, this.CurrentValue);
                            if (this.interactionService.SelectFile(settings, out var newValue))
                            {
                                this.CurrentValue = newValue;
                            }
                        }
                    }
                });
            }
        }
    }
}