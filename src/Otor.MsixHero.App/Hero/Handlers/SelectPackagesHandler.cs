using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using MediatR;
using Otor.MsixHero.App.Hero.Commands.Packages;
using Otor.MsixHero.Appx.Packaging.Installation.Entities;

namespace Otor.MsixHero.App.Hero.Executor.Handlers
{
    public class SelectPackagesHandler : RequestHandler<SelectPackagesCommand>
    {
        private readonly ReaderWriterLockSlim packageListSynchronizer = new ReaderWriterLockSlim();
        private readonly IMsixHeroCommandExecutor commandExecutor;

        public SelectPackagesHandler(IMsixHeroCommandExecutor commandExecutor)
        {
            this.commandExecutor = commandExecutor;
        }

        protected override void Handle(SelectPackagesCommand request)
        {
            IList<InstalledPackage> selected;
            if (!request.SelectedManifestPaths.Any())
            {
                selected = new List<InstalledPackage>();
            }
            else if (request.SelectedManifestPaths.Count == 1)
            {
                try
                {
                    this.packageListSynchronizer.EnterReadLock();
                    var singleSelection = this.commandExecutor.ApplicationState.Packages.AllPackages.FirstOrDefault(a =>
                        string.Equals(a.ManifestLocation, request.SelectedManifestPaths[0],
                            StringComparison.OrdinalIgnoreCase));
                    selected = singleSelection != null
                        ? new List<InstalledPackage> { singleSelection }
                        : new List<InstalledPackage>();
                }
                finally
                {
                    this.packageListSynchronizer.ExitReadLock();
                }
            }
            else
            {
                try
                {
                    this.packageListSynchronizer.EnterReadLock();
                    selected = this.commandExecutor.ApplicationState.Packages.AllPackages.Where(a => request.SelectedManifestPaths.Contains(a.ManifestLocation)).ToList();
                }
                finally
                {
                    this.packageListSynchronizer.ExitReadLock();
                }
            }

            this.commandExecutor.ApplicationState.Packages.SelectedPackages.Clear();
            this.commandExecutor.ApplicationState.Packages.SelectedPackages.AddRange(selected);
        }
    }
}