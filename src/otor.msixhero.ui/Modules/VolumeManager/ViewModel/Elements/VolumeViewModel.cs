using System.Linq;
using otor.msixhero.lib.BusinessLayer.State;
using otor.msixhero.lib.Domain.Appx.Volume;
using otor.msixhero.lib.Domain.Commands.Packages.Grid;
using otor.msixhero.lib.Domain.Commands.Volumes;

namespace otor.msixhero.ui.Modules.VolumeManager.ViewModel.Elements
{
    public class VolumeViewModel : SelectableViewModel<AppxVolume>
    {
        public VolumeViewModel(AppxVolume model, IApplicationStateManager stateManager, bool isSelected = false) : base(model, stateManager, isSelected)
        {
        }

        public bool IsDefault
        {
            get => this.Model.IsDefault;
        }

        public string Name
        {
            get => this.Model.Name;
        }

        public bool IsOffline
        {
            get => this.Model.IsOffline;
        }

        public int OccupiedPercent
        {
            get
            {
                if (this.Model.Capacity == 0)
                {
                    return 100;
                }

                return 100 - (int) (100.0 * this.Model.AvailableFreeSpace / this.Model.Capacity);
            }
        }

        public string CapacityLabel
        {
            get
            {
                if (!this.Model.IsDriveReady)
                {
                    return "Drive not ready";
                }

                if (this.Model.Capacity == 0)
                {
                    return "Capacity unknown";
                }

                var sizeFree = FormatSize(this.Model.AvailableFreeSpace);
                var sizeTotal = FormatSize(this.Model.Capacity);
                return $"{sizeFree} free of {sizeTotal}";
            }
        }

        private static string FormatSize(long sizeInBytes)
        {
            if (sizeInBytes < 1000)
            {
                return sizeInBytes + " B";
            }

            var units = new[] {"TB", "GB", "MB", "KB"};

            for (var i = units.Length - 1; i >= 0; i--)
            {
                sizeInBytes /= 1024;

                if (sizeInBytes < 1024)
                {
                    return sizeInBytes + " " + units[i];
                }
            }

            return sizeInBytes + " " + units[0];
        }

        public string DisplayName
        {
            get => string.IsNullOrWhiteSpace(this.Model.Caption) ? this.Model.PackageStorePath : $"[{this.Model.Caption}] {this.Model.PackageStorePath}";
        }

        public string PackageStorePath
        {
            get => this.Model.PackageStorePath;
        }

        public bool IsThresholdReached
        {
            get => this.Model.Capacity > 0 && this.OccupiedPercent >= 90;
        }

        protected override bool TrySelect()
        {
            if (this.StateManager.CurrentState.Volumes.SelectedItems.Contains(this.Model))
            {
                return false;
            }

            this.StateManager.CommandExecutor.ExecuteAsync(new SelectVolumes(this.Model, SelectionMode.AddToSelection) { IsExplicit = true });
            return true;
        }

        protected override bool TryUnselect()
        {
            if (!this.StateManager.CurrentState.Volumes.SelectedItems.Contains(this.Model))
            {
                return false;
            }

            this.StateManager.CommandExecutor.ExecuteAsync(new SelectVolumes(this.Model, SelectionMode.RemoveFromSelection));
            return true;
        }
    }
}
