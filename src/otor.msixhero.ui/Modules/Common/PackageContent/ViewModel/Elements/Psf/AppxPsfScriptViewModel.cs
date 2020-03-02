using otor.msixhero.lib.Domain.Appx.Psf.Descriptor;
using otor.msixhero.ui.ViewModel;

namespace otor.msixhero.ui.Modules.Common.PackageContent.ViewModel.Elements.Psf
{
    public class AppxPsfScriptViewModel : NotifyPropertyChanged
    {
        private readonly PsfScriptDescriptor descriptor;

        public AppxPsfScriptViewModel(string parentFolder, PsfScriptDescriptor descriptor)
        {
            this.descriptor = descriptor;
            this.FullLocalPath = System.IO.Path.Combine(parentFolder, descriptor.Name);
        }

        public string Name
        {
            get => this.descriptor.Name;
        }

        public string Arguments
        {
            get => this.descriptor.Arguments;
        }

        public PsfScriptDescriptorTiming Timing
        {
            get => this.descriptor.Timing;
        }

        public bool HasArguments
        {
            get => !string.IsNullOrEmpty(this.Arguments);
        }

        public bool InVirtualEnvironment
        {
            get => this.descriptor.RunInVirtualEnvironment;
        }

        public bool WaitForFinish
        {
            get => this.descriptor.WaitForScriptToFinish;
        }

        public bool ShowWindow
        {
            get => this.descriptor.ShowWindow;
        }

        public bool OnlyOnce
        {
            get => this.descriptor.RunOnce;
        }

        public string FullLocalPath { get; }
    }
}
