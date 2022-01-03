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
using System.Collections.ObjectModel;
using System.Windows;
using Microsoft.Xaml.Behaviors;

namespace Otor.MsixHero.App.Helpers.Behaviors
{
    public class BehaviorCollectionEx : Collection<Behavior>
    {
        private WeakReference<DependencyObject> parent;
        // ReSharper disable once NotAccessedField.Local
        private LoadedWeakEventListener weakLoadedEvent;

        // ReSharper disable once NotAccessedField.Local
        private UnloadedWeakEventListener weakUnloadedEvent;

        public BehaviorCollectionEx()
        {
        }
        
        public BehaviorCollectionEx(DependencyObject parent)
        {
            this.parent = new WeakReference<DependencyObject>(parent);

            if (parent is FrameworkElement frameworkElement)
            {
                this.weakLoadedEvent = new LoadedWeakEventListener(frameworkElement, this);
                this.weakUnloadedEvent = new UnloadedWeakEventListener(frameworkElement, this);
            }
        }

        protected override void ClearItems()
        {
            this.DetachAll();
            base.ClearItems();
        }

        protected override void InsertItem(int index, Behavior item)
        {
            base.InsertItem(index, item);
            
            if (item != null && this.parent != null && this.parent.TryGetTarget(out var target))
            {
                item.Attach(target);
            }
        }

        protected override void RemoveItem(int index)
        {
            this[index].Detach();
            base.RemoveItem(index);
        }

        protected override void SetItem(int index, Behavior item)
        {
            var oldItem = this[index];
            base.SetItem(index, item);
            
            oldItem?.Detach();
            if (item != null && this.parent != null && this.parent.TryGetTarget(out var target))
            {
                item.Attach(target);
            }
        }

        internal void SetParent(DependencyObject target)
        {
            this.DetachAll();
            
            if (this.parent == null)
            {
                this.parent = new WeakReference<DependencyObject>(target);
            }
            else
            {
                this.parent.SetTarget(target);
            }
            
            foreach (var behavior in this)
            {
                behavior.Attach(target);
            }

            if (target is FrameworkElement frameworkElement)
            {
                this.weakLoadedEvent = new LoadedWeakEventListener(frameworkElement, this);
                this.weakUnloadedEvent = new UnloadedWeakEventListener(frameworkElement, this);
            }
        }

        public void DetachAll()
        {
            foreach (var behavior in this)
            {
                behavior.Detach();
            }
        }
    }
}