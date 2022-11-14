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

using System.Windows.Media;
using Otor.MsixHero.App.Helpers.Interop;
using Otor.MsixHero.App.Mvvm.Changeable;
using Otor.MsixHero.Infrastructure.Configuration;
using Otor.MsixHero.Infrastructure.Services;

namespace Otor.MsixHero.App.Modules.Dialogs.Settings.Tabs.Commands.ViewModel
{
    public class CommandViewModel : ChangeableContainer
    {
        private readonly ToolListConfiguration _model;

        public CommandViewModel(IInteractionService interactionService, ToolListConfiguration model)
        {
            _model = model;

            AddChildren(
                Path = new ChangeableFileProperty(() => Resources.Localization.Dialogs_Settings_Tools_CommandPath, interactionService, model.Path, ValidatePath),
                Name = new ValidatedChangeableProperty<string>(() => Resources.Localization.Dialogs_Settings_Tools_CommandName, model.Name, ValidateName),
                Icon = new ChangeableFileProperty(() => Resources.Localization.Dialogs_Settings_Tools_CommandIcon, interactionService, model.Icon),
                Arguments = new ChangeableProperty<string>(model.Arguments),
                AsAdmin = new ChangeableProperty<bool>(model.AsAdmin)
            );

            Path.ValueChanged += (_, _) => { OnPropertyChanged(nameof(Image)); };
            Icon.ValueChanged += IconOnValueChanged;
        }

        public ChangeableFileProperty Path { get; }

        public ValidatedChangeableProperty<string> Name { get; }

        public ChangeableProperty<string> Arguments { get; }

        public ChangeableFileProperty Icon { get; }

        public ChangeableProperty<bool> AsAdmin { get; }

        public bool HasIcon => !string.IsNullOrEmpty(Icon.CurrentValue);

        public ImageSource Image => WindowsIcons.GetIconFor(string.IsNullOrEmpty(Icon.CurrentValue) ? Path.CurrentValue : Icon.CurrentValue);

        public static implicit operator ToolListConfiguration(CommandViewModel viewModel)
        {
            return viewModel._model;
        }

        public override void Commit()
        {
            if (Path.IsTouched)
            {
                _model.Path = Path.CurrentValue;
            }

            if (Name.IsTouched)
            {
                _model.Name = Name.CurrentValue;
            }

            if (Icon.IsTouched)
            {
                _model.Icon = Icon.CurrentValue;
            }

            if (AsAdmin.IsTouched)
            {
                _model.AsAdmin = AsAdmin.CurrentValue;
            }

            if (Arguments.IsTouched)
            {
                _model.Arguments = Arguments.CurrentValue;
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
            OnPropertyChanged(nameof(HasIcon));
            OnPropertyChanged(nameof(Image));
        }
    }
}
