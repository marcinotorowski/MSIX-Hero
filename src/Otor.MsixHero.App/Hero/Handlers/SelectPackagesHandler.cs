using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using MediatR;
using Otor.MsixHero.App.Hero.Commands.Packages;
using Otor.MsixHero.App.Hero.Executor;
using Otor.MsixHero.Appx.Packaging;

namespace Otor.MsixHero.App.Hero.Handlers
{
    public class SelectPackagesHandler : RequestHandler<SelectPackagesCommand>
    {
        private readonly ReaderWriterLockSlim _packageListSynchronizer = new ReaderWriterLockSlim();
        private readonly IMsixHeroCommandExecutor _commandExecutor;
        private readonly IMsixHeroApplication _app;

        public SelectPackagesHandler(IMsixHeroCommandExecutor commandExecutor, IMsixHeroApplication app)
        {
            this._commandExecutor = commandExecutor;
            this._app = app;
        }

        protected override void Handle(SelectPackagesCommand request)
        {
            IList<PackageEntry> selected;
            List<string> actualSelection;

            switch (request.SelectionMode)
            {
                case SelectPackagesCommand.PackageSelectionMode.Replace:
                    actualSelection = new List<string>(request.SelectedFullNames);
                    break;
                case SelectPackagesCommand.PackageSelectionMode.Add:
                    actualSelection = new List<string>(this._app.ApplicationState.Packages.SelectedPackages.Select(p => p.PackageFullName).Union(request.SelectedFullNames));
                    break;
                case SelectPackagesCommand.PackageSelectionMode.Remove:
                    actualSelection = new List<string>(this._app.ApplicationState.Packages.SelectedPackages.Select(p => p.PackageFullName).Except(request.SelectedFullNames));
                    break;
                case SelectPackagesCommand.PackageSelectionMode.Toggle:
                    actualSelection = this._app.ApplicationState.Packages.SelectedPackages.Select(p => p.PackageFullName).ToList();

                    foreach (var item in request.SelectedFullNames)
                    {
                        if (actualSelection.Contains(item))
                        {
                            actualSelection.Remove(item);
                        }
                        else
                        {
                            actualSelection.Add(item);
                        }
                    }
                    
                    break;
                default:
                    throw new NotSupportedException();
            }
            
            if (!actualSelection.Any())
            {
                selected = new List<PackageEntry>();
            }
            else if (actualSelection.Count == 1)
            {
                try
                {
                    this._packageListSynchronizer.EnterReadLock();
                    var singleSelection = this._commandExecutor.ApplicationState.Packages.AllPackages.FirstOrDefault(a => string.Equals(a.PackageFullName, actualSelection[0], StringComparison.OrdinalIgnoreCase));
                    selected = singleSelection != null ? new List<PackageEntry> { singleSelection } : new List<PackageEntry>();
                }
                finally
                {
                    this._packageListSynchronizer.ExitReadLock();
                }
            }
            else
            {
                try
                {
                    this._packageListSynchronizer.EnterReadLock();
                    selected = this._commandExecutor.ApplicationState.Packages.AllPackages.Where(a => actualSelection.Contains(a.PackageFullName)).ToList();
                }
                finally
                {
                    this._packageListSynchronizer.ExitReadLock();
                }
            }

            this._commandExecutor.ApplicationState.Packages.SelectedPackages.Clear();
            this._commandExecutor.ApplicationState.Packages.SelectedPackages.AddRange(selected);
        }
    }
}