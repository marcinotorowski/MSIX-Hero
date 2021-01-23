// MSIX Hero
// Copyright (C) 2021 Marcin Otorowski
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

using System.Windows;
using System.Windows.Input;

namespace Otor.MsixHero.App.Mvvm.Commands
{
    /// <summary>
    ///  Allows associated a routed command with a non-routed command.  Used by
    ///  <see cref="RoutedCommandHandlers"/>.
    /// </summary>
    public class RoutedCommandHandler : Freezable
    {
        public static readonly DependencyProperty CommandProperty = DependencyProperty.Register(
            "Command",
            typeof(ICommand),
            typeof(RoutedCommandHandler),
            new PropertyMetadata(default(ICommand)));

        /// <summary> The command that should be executed when the RoutedCommand fires. </summary>
        public ICommand Command
        {
            get => (ICommand)GetValue(CommandProperty);
            set => SetValue(CommandProperty, value);
        }

        /// <summary> The command that triggers <see cref="ICommand"/>. </summary>
        public ICommand RoutedCommand { get; set; }

        /// <inheritdoc />
        protected override Freezable CreateInstanceCore()
        {
            return new RoutedCommandHandler();
        }

        /// <summary>
        ///  Register this handler to respond to the registered RoutedCommand for the
        ///  given element.
        /// </summary>
        /// <param name="owner"> The element for which we should register the command
        ///  binding for the current routed command. </param>
        internal void Register(FrameworkElement owner)
        {
            var binding = new CommandBinding(RoutedCommand, HandleExecuted, HandleCanExecute);
            owner.CommandBindings.Add(binding);
        }

        /// <summary> Proxy to the current Command.CanExecute(object). </summary>
        private void HandleCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = Command?.CanExecute(e.Parameter) == true;
            e.Handled = true;
        }

        /// <summary> Proxy to the current Command.Execute(object). </summary>
        private void HandleExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            Command?.Execute(e.Parameter);
            e.Handled = true;
        }
    }
}
