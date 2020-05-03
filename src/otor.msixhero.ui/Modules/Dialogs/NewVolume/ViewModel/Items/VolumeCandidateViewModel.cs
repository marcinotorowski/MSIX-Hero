using otor.msixhero.lib.Domain.Appx.Volume;
using otor.msixhero.ui.ViewModel;

namespace otor.msixhero.ui.Modules.Dialogs.NewVolume.ViewModel.Items
{
    public class VolumeCandidateViewModel : NotifyPropertyChanged
    {
        public VolumeCandidateViewModel(AppxVolume model)
        {
            this.Model = model;
        }

        public AppxVolume Model { get; }

        public string PackageStorePath => this.Model.PackageStorePath;

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

        public string DisplayName
        {
            get => string.IsNullOrWhiteSpace(this.Model.Caption) ? this.Model.PackageStorePath : $"[{this.Model.Caption}] {this.Model.PackageStorePath}";
        }

        public int OccupiedPercent
        {
            get
            {
                if (this.Model.Capacity == 0)
                {
                    return 100;
                }

                return 100 - (int)(100.0 * this.Model.AvailableFreeSpace / this.Model.Capacity);
            }
        }

        private static string FormatSize(long sizeInBytes)
        {
            if (sizeInBytes < 1000)
            {
                return sizeInBytes + " B";
            }

            var units = new[] { "TB", "GB", "MB", "KB" };

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
    }
}
