﻿// MSIX Hero
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

using System.Windows.Media;
using Otor.MsixHero.App.Helpers.Interop;
using Otor.MsixHero.App.Mvvm.Changeable;
using Otor.MsixHero.Infrastructure.Configuration;
using Otor.MsixHero.Infrastructure.Services;

namespace Otor.MsixHero.App.Modules.Dialogs.Settings.ViewModel.Tabs.Commands
{
    public class CommandViewModel : ChangeableContainer
    {
        private readonly ToolListConfiguration _model;

        public CommandViewModel(IInteractionService interactionService, ToolListConfiguration model)
        {
            this._model = model;
            
            this.AddChildren(
                this.Path = new ChangeableFileProperty(() => Resources.Localization.Dialogs_Settings_Tools_CommandPath, interactionService, model.Path, ValidatePath),
                this.Name = new ValidatedChangeableProperty<string>(() => Resources.Localization.Dialogs_Settings_Tools_CommandName, model.Name, ValidateName),
                this.Icon = new ChangeableFileProperty(() => Resources.Localization.Dialogs_Settings_Tools_CommandIcon, interactionService, model.Icon),
                this.Arguments = new ChangeableProperty<string>(model.Arguments),
                this.AsAdmin = new ChangeableProperty<bool>(model.AsAdmin)
            );

            this.Path.ValueChanged += (_, _) => { this.OnPropertyChanged(nameof(Image)); };
            this.Icon.ValueChanged += this.IconOnValueChanged;
        }

        public ChangeableFileProperty Path { get; }

        public ValidatedChangeableProperty<string> Name { get; }
        
        public ChangeableProperty<string> Arguments { get; }

        public ChangeableFileProperty Icon { get; }

        public ChangeableProperty<bool> AsAdmin { get; }

        public bool HasIcon => !string.IsNullOrEmpty(this.Icon.CurrentValue);

        public ImageSource Image => WindowsIcons.GetIconFor(string.IsNullOrEmpty(this.Icon.CurrentValue) ? this.Path.CurrentValue : this.Icon.CurrentValue);

        public static implicit operator ToolListConfiguration(CommandViewModel viewModel)
        {
            return viewModel._model;
        }

        public override void Commit()
        {
            if (this.Path.IsTouched)
            {
                this._model.Path = this.Path.CurrentValue;
            }

            if (this.Name.IsTouched)
            {
                this._model.Name = this.Name.CurrentValue;
            }

            if (this.Icon.IsTouched)
            {
                this._model.Icon = this.Icon.CurrentValue;
            }

            if (this.AsAdmin.IsTouched)
            {
                this._model.AsAdmin = this.AsAdmin.CurrentValue;
            }

            if (this.Arguments.IsTouched)
            {
                this._model.Arguments = this.Arguments.CurrentValue;
            }

            base.Commit();
        }

        private static string ValidatePath(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                return Resources.Localization.Dialogs_Settings_Tools_Validation_EmptyPath;
            }

            return null;
        }

        private static string ValidateName(string value)
        {
            if (!string.IsNullOrEmpty(value))
            {
                return null;
            }
            
            return Resources.Localization.Dialogs_Settings_Tools_Validation_EmptyName;
        }

        private void IconOnValueChanged(object sender, ValueChangedEventArgs e)
        {
            this.OnPropertyChanged(nameof(HasIcon));
            this.OnPropertyChanged(nameof(Image));
        }
    }
}
