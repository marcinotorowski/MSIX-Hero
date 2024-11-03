using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Otor.MsixHero.App.Hero.Commands.Packages;
using Otor.MsixHero.App.Hero.Executor;
using Otor.MsixHero.Appx.Packaging;

namespace Otor.MsixHero.App.Hero.Handlers
{
    public class SelectPackagesHandler : IRequestHandler<SelectPackagesCommand>
    {
        private readonly ReaderWriterLockSlim _packageListSynchronizer = new ReaderWriterLockSlim();
        private readonly IMsixHeroCommandExecutor _commandExecutor;
        private readonly IMsixHeroApplication _app;

        public SelectPackagesHandler(IMsixHeroCommandExecutor commandExecutor, IMsixHeroApplication app)
        {
            this._commandExecutor = commandExecutor;
            this._app = app;
        }

        Task IRequestHandler<SelectPackagesCommand>.Handle(SelectPackagesCommand request, CancellationToken cancellationToken)
        {
            IList<PackageEntry> selected;
            List<PackageLUID> actualSelection;

            switch (request.SelectionMode)
            {
                case SelectPackagesCommand.PackageSelectionMode.Replace:
                    actualSelection = new List<PackageLUID>(request.SelectedIds);
                    break;
                case SelectPackagesCommand.PackageSelectionMode.Add:
                    actualSelection = new List<PackageLUID>(this._app.ApplicationState.Packages.SelectedPackages.Select(p => new PackageLUID(p)).Union(request.SelectedIds));
                    break;
                case SelectPackagesCommand.PackageSelectionMode.Remove:
                    actualSelection = new List<PackageLUID>(this._app.ApplicationState.Packages.SelectedPackages.Select(p => new PackageLUID(p)).Except(request.SelectedIds));
                    break;
                case SelectPackagesCommand.PackageSelectionMode.Toggle:
                    actualSelection = this._app.ApplicationState.Packages.SelectedPackages.Select(p => new PackageLUID(p)).ToList();

                    foreach (var item in request.SelectedIds)
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
                    var singleSelection = this._commandExecutor.ApplicationState.Packages.AllPackages.FirstOrDefault(a => new PackageLUID(a).Equals(actualSelection[0]));
                    selected = singleSelection != null ? [singleSelection] : new List<PackageEntry>();
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
                    selected = this._commandExecutor.ApplicationState.Packages.AllPackages.Where(a => actualSelection.Contains(new PackageLUID(a))).ToList();
                }
                finally
                {
                    this._packageListSynchronizer.ExitReadLock();
                }
            }

            this._commandExecutor.ApplicationState.Packages.SelectedPackages.Clear();
            this._commandExecutor.ApplicationState.Packages.SelectedPackages.AddRange(selected);

            return Task.CompletedTask;
        }
    }
}