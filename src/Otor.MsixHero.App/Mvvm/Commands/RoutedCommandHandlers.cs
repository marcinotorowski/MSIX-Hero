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

using System.Collections;
using System.Collections.Specialized;
using System.Windows;

namespace Otor.MsixHero.App.Mvvm.Commands
{
    /// <summary>
    ///  Holds a collection of <see cref="RoutedCommandHandler"/> that should be
    ///  turned into CommandBindings.
    /// </summary>
    public class RoutedCommandHandlers : FreezableCollection<RoutedCommandHandler>
    {
        /// <summary>
        ///  Hide this from WPF so that it's forced to go through
        ///  <see cref="GetCommands"/> and we can auto-create the collection
        ///  if it doesn't already exist.  This isn't strictly necessary but it makes
        ///  the XAML much nicer.
        /// </summary>
        private static readonly DependencyProperty CommandsProperty = DependencyProperty.RegisterAttached(
          "CommandsPrivate",
          typeof(RoutedCommandHandlers),
          typeof(RoutedCommandHandlers),
          new PropertyMetadata(default(RoutedCommandHandlers)));

        /// <summary>
        ///  Gets the collection of RoutedCommandHandler for a given element, creating
        ///  it if it doesn't already exist.
        /// </summary>
        public static RoutedCommandHandlers GetCommands(FrameworkElement element)
        {
            RoutedCommandHandlers handlers = (RoutedCommandHandlers)element.GetValue(CommandsProperty);
            if (handlers == null)
            {
                handlers = new RoutedCommandHandlers(element);
                element.SetValue(CommandsProperty, handlers);
            }

            return handlers;
        }

        private readonly FrameworkElement _owner;

        /// <summary> Each collection is tied to a specific element. </summary>
        /// <param name="owner"> The element for which this collection is created. </param>
        public RoutedCommandHandlers(FrameworkElement owner)
        {
            _owner = owner;

            // because we auto-create the collection, we don't know when items will be
            // added.  So, we observe ourself for changes manually. 
            var self = (INotifyCollectionChanged)this;
            self.CollectionChanged += (sender, args) =>
            {
                // note this does not handle deletions, that's left as an exercise for the
                // reader, but most of the time, that's not needed! 
                ((RoutedCommandHandlers)sender).HandleAdditions(args.NewItems);
            };
        }

        /// <summary> Invoked when new items are added to the collection. </summary>
        /// <param name="newItems"> The new items that were added. </param>
        private void HandleAdditions(IList newItems)
        {
            if (newItems == null)
                return;

            foreach (RoutedCommandHandler routedHandler in newItems)
            {
                routedHandler.Register(_owner);
            }
        }

        /// <inheritdoc />
        protected override Freezable CreateInstanceCore()
        {
            return new RoutedCommandHandlers(_owner);
        }
    }
}
