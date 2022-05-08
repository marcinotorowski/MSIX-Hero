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
using System.Linq;
using System.Threading;
using Windows.System;
using Dapplo.Log;

namespace Otor.MsixHero.Appx.Diagnostic.RunningDetector
{
    public class RunningAppsDetector : IRunningAppsDetector
    {
        private static readonly LogSource Logger = new();
        private readonly IList<Subscriber> _observers = new List<Subscriber>();

        private readonly ReaderWriterLockSlim _syncWriterLockSlim = new ReaderWriterLockSlim();
        private HashSet<string> _activeApps;
        private AppDiagnosticInfoWatcher _watcher;

        public IDisposable Subscribe(IObserver<ActivePackageFullNames> observer)
        {
            var returned = new Subscriber(this, observer);
            _observers.Add(returned);
            return returned;
        }
        
        public IList<string> GetCurrentlyRunningPackageNames()
        {
            Logger.Info().WriteLine("Getting the list of running apps...");

            try
            {
                Logger.Verbose().WriteLine("Entering upgradeable read lock...");
                this._syncWriterLockSlim.EnterUpgradeableReadLock();

                if (this._watcher == null)
                {
                    Logger.Info().WriteLine("Starting listening on background apps activity...");
                    this._watcher = AppDiagnosticInfo.CreateWatcher();
                    this._watcher.Start();
                }
                else
                {
                    this._watcher.Removed -= this.WatcherOnRemoved;
                    this._watcher.Added -= this.WatcherOnAdded;
                }

                if (this._activeApps != null)
                {
                    return this._activeApps.ToList();
                }

                Logger.Verbose().WriteLine("Upgrading to write lock...");
                this._syncWriterLockSlim.EnterWriteLock();
                
                try
                {
                    this._activeApps = new HashSet<string>(StringComparer.Ordinal);
                    
                    foreach (var item in AppDiagnosticInfo.RequestInfoAsync().GetAwaiter().GetResult())
                    {
                        var family = GetFamilyNameFromAppUserModelId(item.AppInfo.AppUserModelId);
                        this._activeApps.Add(family);
                    }

                    Logger.Info().WriteLine($"Returning {this._activeApps.Count} apps running in the background...");
                }
                finally
                {
                    this._syncWriterLockSlim.ExitWriteLock();
                }

                this.Publish(new ActivePackageFullNames(this._activeApps));
                return this._activeApps.ToList();
            }
            finally
            {
                if (this._watcher != null)
                {
                    this._watcher.Removed += this.WatcherOnRemoved;
                    this._watcher.Added += this.WatcherOnAdded;
                }

                Logger.Verbose().WriteLine("Exiting upgradeable read lock...");
                this._syncWriterLockSlim.ExitUpgradeableReadLock();
            }
        }

        private static string GetFamilyNameFromAppUserModelId(string appInfoAppUserModelId)
        {
            var exclamation = appInfoAppUserModelId.LastIndexOf('!');
            if (exclamation == -1)
            {
                return appInfoAppUserModelId;
            }

            // we need to cut-out the entry point
            return appInfoAppUserModelId.Substring(0, exclamation);
        }
        
        private void StopListening()
        {
            Logger.Info().WriteLine("Stopping listening on background apps activity...");
            this._watcher.Stop();
            this._watcher.Added -= this.WatcherOnAdded;
            this._watcher.Removed -= this.WatcherOnRemoved;
            this._watcher = null;
        }

        private void WatcherOnAdded(AppDiagnosticInfoWatcher sender, AppDiagnosticInfoWatcherEventArgs args)
        {
            Logger.Verbose().WriteLine($"The following app has been started: {args.AppDiagnosticInfo.AppInfo.AppUserModelId}");
            this._syncWriterLockSlim.EnterWriteLock();

            try
            {
                var family = GetFamilyNameFromAppUserModelId(args.AppDiagnosticInfo.AppInfo.AppUserModelId);

                if (!this._activeApps.Add(family))
                {
                    return;
                }

                Logger.Debug().WriteLine($"The following app family name has been started: {args.AppDiagnosticInfo.AppInfo.AppUserModelId}");
                this.Publish(new ActivePackageFullNames(this._activeApps));
            }
            finally
            {
                this._syncWriterLockSlim.ExitWriteLock();
            }
        }

        private void WatcherOnRemoved(AppDiagnosticInfoWatcher sender, AppDiagnosticInfoWatcherEventArgs args)
        {
            Logger.Verbose().WriteLine($"The following app has been closed: {args.AppDiagnosticInfo.AppInfo.AppUserModelId}");
            this._syncWriterLockSlim.EnterWriteLock();

            try
            {
                var family = GetFamilyNameFromAppUserModelId(args.AppDiagnosticInfo.AppInfo.AppUserModelId);

                if (!this._activeApps.Remove(family))
                {
                    return;
                }

                Logger.Debug().WriteLine($"The following app family name has been closed: {args.AppDiagnosticInfo.AppInfo.AppUserModelId}");
                this.Publish(new ActivePackageFullNames(this._activeApps));
            }
            finally
            {
                this._syncWriterLockSlim.ExitWriteLock();
            }
        }
        
        private void Publish(ActivePackageFullNames eventPayload)
        {
            foreach (var item in this._observers)
            {
                item.Notify(eventPayload);
            }
        }

        void IDisposable.Dispose()
        {
            this.StopListening();

            this._syncWriterLockSlim.Dispose();

            foreach (var disposable in this._observers)
            {
                disposable.Dispose();
            }
        }

        private class Subscriber : IDisposable
        {
            private readonly RunningAppsDetector _parent;
            private readonly IObserver<ActivePackageFullNames> _observer;

            public Subscriber(RunningAppsDetector parent, IObserver<ActivePackageFullNames> observer)
            {
                this._parent = parent;
                this._observer = observer;
            }

            public void Notify(ActivePackageFullNames payload)
            {
                this._observer.OnNext(payload);
            }

            public void Dispose()
            {
                _parent._observers.Remove(this);
            }
        }
    }
}
