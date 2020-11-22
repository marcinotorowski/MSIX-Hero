using System;
using System.Collections.Generic;
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
    public class RunningAppsDetector : IRunningAppsDetector, IDisposable
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(RunningAppsDetector));
        private readonly IList<Subscriber> observers = new List<Subscriber>();
        private readonly AutoResetEvent syncObject = new AutoResetEvent(true);
        private readonly ManualResetEvent resultsAvailability = new ManualResetEvent(true);
        private readonly ReaderWriterLockSlim readerWriter = new ReaderWriterLockSlim();

        private readonly Timer timer = new Timer(2000);
        private HashSet<string> previouslyRunningAppIds;

        public IDisposable Subscribe(IObserver<ActivePackageFullNames> observer)
        {
            var returned = new Subscriber(this, observer);
            observers.Add(returned);
            return returned;
        }

        public IEnumerable<InstalledPackage> GetCurrentlyRunning(IEnumerable<InstalledPackage> allPackages)
        {
            this.resultsAvailability.WaitOne();

            this.readerWriter.EnterReadLock();
            try
            {
                return allPackages.Where(p => this.previouslyRunningAppIds.Contains(p.PackageId)).ToArray();
            }
            finally
            {
                this.readerWriter.ExitReadLock();
            }
        }

        public IEnumerable<string> GetCurrentlyRunningPackageNames()
        {
            this.resultsAvailability.WaitOne();

            this.readerWriter.EnterReadLock();
            try
            {
                return this.previouslyRunningAppIds.ToArray();
            }
            finally
            {
                this.readerWriter.ExitReadLock();
            }
        }

        public void StartListening()
        {
            var waited = this.syncObject.WaitOne();

            try
            {
                this.resultsAvailability.Reset();

                if (this.timer.Enabled)
                {
                    this.timer.Elapsed -= this.TimerOnElapsed;
                    this.timer.Stop();
                }

                this.timer.Elapsed += this.TimerOnElapsed;
                this.timer.Start();
            }
            finally
            {
                if (waited)
                {
                    this.syncObject.Set();
                }
            }
        }

        private static HashSet<string> GetNowRunning()
        {
            IList<TaskListWrapper.AppProcess> table;
            try
            {
                var wrapper = new TaskListWrapper();
                table = wrapper.GetBasicAppProcesses("running").ConfigureAwait(false).GetAwaiter().GetResult();
            }
            catch (Exception e)
            {
                Logger.Warn(e, "Could not get the list of running processes.");
                return null;
            }

            var nowRunning = new HashSet<string>();
            foreach (var item in table)
            {
                Logger.Debug($"PID = {item.ProcessId}, Name = {item.PackageName}, Memory = {item.MemoryUsage}, Image = {item.ImageName}");
                nowRunning.Add(item.PackageName);
            }

            return nowRunning;
        }

        private async void TimerOnElapsed(object sender, ElapsedEventArgs e)
        {
#pragma warning disable 4014
            this.timer.Stop();
            try
            {
                await Task.Run(this.DoCheck).ConfigureAwait(false);
            }
            finally
            {
                this.timer.Start();
            }
#pragma warning restore 4014
        }

        private void DoCheck()
        {
            var wait = this.syncObject.WaitOne();

            Logger.Debug("Checking for running apps.");
            try
            {
                this.readerWriter.EnterWriteLock();
                this.resultsAvailability.Reset();

                var nowRunning = GetNowRunning();
                if (nowRunning == null)
                {
                    return;
                }

                if (this.previouslyRunningAppIds != null && nowRunning.SequenceEqual(this.previouslyRunningAppIds))
                {
                    Logger.Trace("The list of running apps has not changed.");
                    return;
                }

                Logger.Info("Notifying about updated app state.");
                this.previouslyRunningAppIds = nowRunning;
            }
            catch (Exception e)
            {
                Logger.Error(e);
                throw;
            }
            finally
            {
                this.resultsAvailability.Set();
                this.readerWriter.ExitWriteLock();

                if (wait)
                {
                    this.syncObject.Set();
                }
            }

            this.Publish(new ActivePackageFullNames(this.previouslyRunningAppIds));
        }

        public void StopListening()
        {
            var wait = this.syncObject.WaitOne();

            try
            {
                this.timer.Elapsed -= this.TimerOnElapsed;
                this.timer.Stop();
                this.resultsAvailability.Set();
            }
            finally
            {
                if (wait)
                {
                    this.syncObject.Set();
                }
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
