using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using otor.msixhero.lib.Domain.Appx.Psf.Descriptor;
using otor.msixhero.ui.ViewModel;

namespace otor.msixhero.ui.Modules.Common.PackageContent.ViewModel.Elements.Psf
{
    public class AppxPsfViewModel : NotifyPropertyChanged
    {
        private readonly PsfApplicationDescriptor definition;

        public static readonly ICommand EditScript = new RoutedUICommand() { Text = "Edit script" };

        public AppxPsfViewModel(string installFolder, PsfApplicationDescriptor definition)
        {
            this.definition = definition;

            if (this.definition.Tracing != null)
            {
                this.Tracing = new TracingPsfViewModel(this.definition.Tracing);
            }

            if (this.definition.Scripts != null)
            {
                this.Scripts = new List<AppxPsfScriptViewModel>(definition.Scripts.Select(s => new AppxPsfScriptViewModel(installFolder, s)));
            }
        }

        public string Executable => this.definition.Executable;
        
        public string Arguments => this.definition.Arguments;
        
        public string WorkingDirectory => this.definition.WorkingDirectory;
        
        public List<PsfFolderRedirectionDescriptor> FileRedirections => this.definition.FileRedirections;
        
        public List<AppxPsfScriptViewModel> Scripts { get; }

        public List<string> OtherFixups => this.definition.OtherFixups;

        public bool HasArguments => !string.IsNullOrEmpty(this.definition.Arguments);

        public bool IsAdvanced => this.HasArguments || this.HasTracing || this.HasWorkingDirectory || this.HasFileRedirections || this.HasOtherFixups;

        public bool HasWorkingDirectory => !string.IsNullOrEmpty(this.definition.WorkingDirectory);

        public TracingPsfViewModel Tracing { get; }

        public bool HasFileRedirections => this.definition.FileRedirections?.Count > 0;
        
        public bool HasScripts => this.definition.Scripts?.Count > 0;

        public bool HasOtherFixups => this.definition.OtherFixups?.Count > 0;

        public bool HasTracing => this.Tracing != null;
    }
}