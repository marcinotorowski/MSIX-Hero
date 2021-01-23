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
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using Otor.MsixHero.Appx.Packaging.Installation.Entities;
using Otor.MsixHero.Infrastructure.Logging;
using Otor.MsixHero.Infrastructure.ThirdParty.Sdk;
using Timer = System.Timers.Timer;

namespace Otor.MsixHero.Appx.Diagnostic.RunningDetector
{
    public class RunningDetector : IRunningDetector, IDisposable
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(RunningDetector));
        private readonly IList<Subscriber> observers = new List<Subscriber>();

        private readonly Timer timer = new Timer(2000);
        private IDictionary<string, string> consideredPackages;
        private HashSet<string> previouslyRunningAppIds;

        public IDisposable Subscribe(IObserver<ActivePackageFullNames> observer)
        {
            var returned = new Subscriber(this, observer);
            observers.Add(returned);
            return returned;
        }

        public async Task Listen(IList<InstalledPackage> installedPackages, CancellationToken cancellationToken)
        {
            var path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "WindowsApps") + "\\";
            this.consideredPackages = new Dictionary<string, string>();

            foreach (var item in installedPackages.Where(p => p.ManifestLocation?.StartsWith(path, StringComparison.OrdinalIgnoreCase) == true))
            {
                this.consideredPackages[item.ManifestLocation.Substring(0, item.ManifestLocation.IndexOf('\\', path.Length + 1))] = item.PackageId;
            }

            this.timer.Elapsed -= this.TimerOnElapsed;
            this.timer.Elapsed += this.TimerOnElapsed;
            this.timer.Start();

            await this.DoCheck().ConfigureAwait(false);
        }

        private void TimerOnElapsed(object sender, ElapsedEventArgs e)
        {
#pragma warning disable 4014
            this.DoCheck();
#pragma warning restore 4014
        }

        private async Task DoCheck()
        {
            if (!this.timer.Enabled)
            {
                return;
            }

            if (this.consideredPackages == null)
            {
                return;
            }

            Logger.Debug("Checking for running apps.");
            try
            {
                this.timer.Stop();

                var wrapper = new TaskListWrapper();
                IList<TaskListWrapper.AppProcess> table;
                try
                {
                    table = await wrapper.GetBasicAppProcesses("running").ConfigureAwait(false);
                }
                catch (Exception e)
                {
                    Logger.Warn(e, "Could not get the list of running processes.");
                    return;
                }

                var nowRunning = new HashSet<string>();
                foreach (var item in table)
                {
                    Logger.Debug($"PID = {item.ProcessId}, Name = {item.PackageName}, Memory = {item.MemoryUsage}, Image = {item.ImageName}");
                    nowRunning.Add(item.PackageName);
                }

                if (this.previouslyRunningAppIds != null && nowRunning.SequenceEqual(this.previouslyRunningAppIds))
                {
                    Logger.Debug("The list of running apps has not changed.");
                    return;
                }

                Logger.Info("Notifying about updated app state.");
                this.previouslyRunningAppIds = nowRunning;
                this.Publish(new ActivePackageFullNames(nowRunning));
            }
            catch (Exception e)
            {
                Logger.Error(e);
                throw;
            }
            finally
            {
                this.timer.Start();
            }
        }

        public Task StopListening(CancellationToken cancellationToken)
        {
            this.consideredPackages = null;
            this.timer.Elapsed -= this.TimerOnElapsed;
            this.timer.Stop();
            return Task.FromResult(true);
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
            foreach (var disposable in this.observers)
            {
                disposable.Dispose();
            }
        }

        private class Subscriber : IDisposable
        {
            private readonly RunningDetector parent;
            private readonly IObserver<ActivePackageFullNames> observer;

            public Subscriber(RunningDetector parent, IObserver<ActivePackageFullNames> observer)
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
