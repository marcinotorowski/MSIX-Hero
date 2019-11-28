using System.Collections.Generic;
using otor.msixhero.lib.Domain.Appx.Psf;
using otor.msixhero.ui.ViewModel;

namespace otor.msixhero.ui.Modules.PackageList.ViewModel
{
    public class AppxPsfViewModel : NotifyPropertyChanged
    {
        private readonly PsfApplicationDefinition definition;

        public AppxPsfViewModel(PsfApplicationDefinition definition)
        {
            this.definition = definition;
        }

        public string Executable => this.definition.Executable;
        
        public string Arguments => this.definition.Arguments;
        
        public string WorkingDirectory => this.definition.WorkingDirectory;
        
        public List<PsfFileRedirection> FileRedirections => this.definition.FileRedirections;

        public List<string> OtherFixups => this.definition.OtherFixups;

        public bool HasArguments => !string.IsNullOrEmpty(this.definition.Arguments);

        public bool IsAdvanced => this.HasArguments || this.HasTracing || this.HasWorkingDirectory || this.HasFileRedirections || this.HasOtherFixups;

        public bool HasWorkingDirectory => !string.IsNullOrEmpty(this.definition.WorkingDirectory);

        public bool HasFileRedirections => this.definition.FileRedirections?.Count > 0;

        public bool HasOtherFixups => this.definition.OtherFixups?.Count > 0;

        public bool HasTracing => this.definition.Tracing.HasValue;

        public string DisplayTracing
        {
            get
            {
                if (this.definition.Tracing == PsfBitness.x64)
                {
                    return "Yes, 64-bit";
                }

                if (this.definition.Tracing == PsfBitness.x64)
                {
                    return "Yes, 32-bit";
                }

                return "Yes";
            }
        }
    }
}