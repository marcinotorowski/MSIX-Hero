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

using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Otor.MsixHero.App.Modules.Dialogs.Packaging.SharedAppContainer.ViewModel;

namespace Otor.MsixHero.App.Modules.Dialogs.Packaging.SharedAppContainer.View
{
    public partial class SharedAppContainerDialogContent
    {

        public SharedAppContainerDialogContent()
        {
            this.InitializeComponent();
        }

        private void ListBox_OnMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var listBox = (ListBox)sender;
            
            for (var i = 0; i < listBox.Items.Count; i++)
            {
                var container = (ListBoxItem)listBox.ItemContainerGenerator.ContainerFromIndex(i);
                var viewModel = (SharedAppViewModel)container.DataContext;
                if (container.IsMouseOver)
                {
                    viewModel.IsEditing = true;
                }
                else
                {
                    viewModel.IsEditing = false;
                }
            }
        }

        private void TextBox_OnKeyDown(object sender, KeyEventArgs e)
        {
            var textBox = (TextBox)sender;
            var viewModel = (SharedAppViewModel)textBox.DataContext;

            switch (e.Key)
            {
                case Key.Enter:
                    var dataContext = (SharedAppContainerViewModel)this.DataContext;
                    viewModel.SetFromFamilyName(dataContext.PackageQuery, textBox.Text, CancellationToken.None).GetAwaiter().GetResult();
                    viewModel.IsEditing = false;
                    break;
                case Key.Escape:
                    viewModel.IsEditing = false;
                    break;
            }
        }

        private void TextBox_OnIsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (!(bool)e.NewValue)
            {
                return;
            }

            var textBox = (TextBox)sender;
            var viewModel = (SharedAppViewModel)textBox.DataContext;
            if (viewModel == null)
            {
                return;
            }

            textBox.Text = viewModel.FamilyName.CurrentValue;

            Dispatcher.BeginInvoke(() =>
            {
                var window = Window.GetWindow(textBox);
                if (window == null)
                {
                    return;
                }

                textBox.Focus();
                Keyboard.Focus(textBox);
                FocusManager.SetFocusedElement(window, textBox);
                textBox.SelectAll();
            });
        }
    }
}
