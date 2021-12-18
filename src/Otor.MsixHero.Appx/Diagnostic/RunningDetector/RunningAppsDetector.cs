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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Windows.System;
using Otor.MsixHero.Infrastructure.Logging;

namespace Otor.MsixHero.Appx.Diagnostic.RunningDetector
{
    public class RunningAppsDetector : IRunningAppsDetector
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(RunningAppsDetector));

        private readonly IList<Subscriber> observers = new List<Subscriber>();

        private readonly ReaderWriterLockSlim syncWriterLockSlim = new ReaderWriterLockSlim();
        private HashSet<string> activeApps;
        private AppDiagnosticInfoWatcher watcher;

        public IDisposable Subscribe(IObserver<ActivePackageFullNames> observer)
        {
            var returned = new Subscriber(this, observer);
            observers.Add(returned);
            return returned;
        }
        
        public IList<string> GetCurrentlyRunningPackageNames()
        {
            Logger.Info("Getting the list of running apps...");

            try
            {
                Logger.Trace("Entering upgradeable read lock...");
                this.syncWriterLockSlim.EnterUpgradeableReadLock();

                if (this.watcher == null)
                {
                    Logger.Info("Starting listening on background apps activity...");
                    this.watcher = AppDiagnosticInfo.CreateWatcher();
                    this.watcher.Start();
                }
                else
                {
                    this.watcher.Removed -= this.WatcherOnRemoved;
                    this.watcher.Added -= this.WatcherOnAdded;
                }

                if (this.activeApps != null)
                {
                    return this.activeApps.ToList();
                }

                Logger.Trace("Upgrading to write lock...");
                this.syncWriterLockSlim.EnterWriteLock();
                
                try
                {
                    this.activeApps = new HashSet<string>(StringComparer.Ordinal);
                    
                    foreach (var item in AppDiagnosticInfo.RequestInfoAsync().GetAwaiter().GetResult())
                    {
                        var family = GetFamilyNameFromAppUserModelId(item.AppInfo.AppUserModelId);
                        this.activeApps.Add(family);
                    }

                    Logger.Info($"Returning {this.activeApps.Count} apps running in the background...");
                }
                finally
                {
                    this.syncWriterLockSlim.ExitWriteLock();
                }

                this.Publish(new ActivePackageFullNames(this.activeApps));
                return this.activeApps.ToList();
            }
            finally
            {
                if (this.watcher != null)
                {
                    this.watcher.Removed += this.WatcherOnRemoved;
                    this.watcher.Added += this.WatcherOnAdded;
                }

                Logger.Trace("Exiting upgradeable read lock...");
                this.syncWriterLockSlim.ExitUpgradeableReadLock();
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
            Logger.Info("Stopping listening on background apps activity...");
            this.watcher.Stop();
            this.watcher.Added -= this.WatcherOnAdded;
            this.watcher.Removed -= this.WatcherOnRemoved;
            this.watcher = null;
        }

        private void WatcherOnAdded(AppDiagnosticInfoWatcher sender, AppDiagnosticInfoWatcherEventArgs args)
        {
            Logger.Trace($"The following app has been started: {args.AppDiagnosticInfo.AppInfo.AppUserModelId}");
            this.syncWriterLockSlim.EnterWriteLock();

            try
            {
                var family = GetFamilyNameFromAppUserModelId(args.AppDiagnosticInfo.AppInfo.AppUserModelId);

                if (!this.activeApps.Add(family))
                {
                    return;
                }

                Logger.Debug($"The following app family name has been started: {args.AppDiagnosticInfo.AppInfo.AppUserModelId}");
                this.Publish(new ActivePackageFullNames(this.activeApps));
            }
            finally
            {
                this.syncWriterLockSlim.ExitWriteLock();
            }
        }

        private void WatcherOnRemoved(AppDiagnosticInfoWatcher sender, AppDiagnosticInfoWatcherEventArgs args)
        {
            Logger.Trace($"The following app has been closed: {args.AppDiagnosticInfo.AppInfo.AppUserModelId}");
            this.syncWriterLockSlim.EnterWriteLock();

            try
            {
                var family = GetFamilyNameFromAppUserModelId(args.AppDiagnosticInfo.AppInfo.AppUserModelId);

                if (!this.activeApps.Remove(family))
                {
                    return;
                }

                Logger.Debug($"The following app family name has been closed: {args.AppDiagnosticInfo.AppInfo.AppUserModelId}");
                this.Publish(new ActivePackageFullNames(this.activeApps));
            }
            finally
            {
                this.syncWriterLockSlim.ExitWriteLock();
            }
        }
        
        private void Publish(ActivePackageFullNames eventPayload)
        {
            foreach (var item in this.observers)
            {
                item.Notify(eventPayload);
            }
        }

        void IDisposable.Dispose()
        {
            this.StopListening();

            this.syncWriterLockSlim.Dispose();

            foreach (var disposable in this.observers)
            {
                disposable.Dispose();
            }
        }

        private class Subscriber : IDisposable
        {
            private readonly RunningAppsDetector parent;
            private readonly IObserver<ActivePackageFullNames> observer;

            public Subscriber(RunningAppsDetector parent, IObserver<ActivePackageFullNames> observer)
            {
                this.parent = parent;
                this.observer = observer;
            }

            public void Notify(ActivePackageFullNames payload)
            {
                this.observer.OnNext(payload);
            }

            public void Dispose()
            {
                parent.observers.Remove(this);
            }
        }
    }
}
