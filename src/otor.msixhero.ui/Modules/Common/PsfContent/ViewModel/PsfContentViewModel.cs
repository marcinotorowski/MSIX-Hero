using System.Collections.Specialized;
using System.Linq;
using otor.msixhero.lib.Domain.Appx.Psf;
using otor.msixhero.ui.Domain;
using otor.msixhero.ui.Modules.Common.PsfContent.ViewModel.Items.Electron;
using otor.msixhero.ui.Modules.Common.PsfContent.ViewModel.Items.Redirection;
using otor.msixhero.ui.Modules.Common.PsfContent.ViewModel.Items.Trace;

namespace otor.msixhero.ui.Modules.Common.PsfContent.ViewModel
{
    public class PsfContentViewModel : ChangeableContainer
    {
        public PsfContentViewModel(PsfConfig psfConfig)
        {
            this.RedirectionRules = new ChangeableCollection<PsfContentProcessRedirectionViewModel>();
            this.TraceRules = new ChangeableCollection<PsfContentProcessTraceViewModel>();
            this.ElectronRules = new ChangeableCollection<PsfContentProcessElectronViewModel>();

            this.Setup(psfConfig);

            this.RedirectionRules.Commit();
            this.TraceRules.Commit();
            this.ElectronRules.Commit();
            this.AddChildren(this.RedirectionRules, this.TraceRules, this.ElectronRules);

            this.RedirectionRules.CollectionChanged += this.RedirectionRulesOnCollectionChanged;
            this.ElectronRules.CollectionChanged += this.ElectronRulesOnCollectionChanged;
            this.TraceRules.CollectionChanged += this.TraceRulesOnCollectionChanged;
        }

        public ChangeableCollection<PsfContentProcessRedirectionViewModel> RedirectionRules { get; }

        public ChangeableCollection<PsfContentProcessTraceViewModel> TraceRules { get; }

        public ChangeableCollection<PsfContentProcessElectronViewModel> ElectronRules { get; }

        public bool HasTraceRules => this.TraceRules.Any();

        public bool HasRedirectionRules => this.RedirectionRules.Any();

        public bool HasElectronRules => this.RedirectionRules.Any();

        public bool HasPsf { get; private set; }

        private void Setup(PsfConfig psfConfig)
        {
            var hadPsf = this.HasPsf;

            if (psfConfig?.Processes != null)
            {
                foreach (var process in psfConfig.Processes)
                {
                    foreach (var item in process.Fixups.Where(f => f.Config != null))
                    {
                        if (item.Config is PsfRedirectionFixupConfig redirectionConfig)
                        {
                            var psfContentProcessViewModel = new PsfContentProcessRedirectionViewModel(process.Executable, item.Dll, redirectionConfig.RedirectedPaths);
                            this.RedirectionRules.Add(psfContentProcessViewModel);
                            this.HasPsf = true;
                        }
                        else if (item.Config is PsfTraceFixupConfig traceConfig)
                        {
                            var psfContentProcessViewModel = new PsfContentProcessTraceViewModel(process.Executable, item.Dll, traceConfig);
                            this.TraceRules.Add(psfContentProcessViewModel);
                            this.HasPsf = true;
                        }
                        else if (item.Config is PsfElectronFixupConfig electronConfig)
                        {
                            var psfContentProcessViewModel = new PsfContentProcessElectronViewModel(process.Executable, item.Dll, electronConfig);
                            this.ElectronRules.Add(psfContentProcessViewModel);
                            this.HasPsf = true;
                        }
                    }
                }
            }

            if (hadPsf != this.HasPsf)
            {
                this.OnPropertyChanged(nameof(HasPsf));
            }
        }

        private void RedirectionRulesOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            this.OnPropertyChanged(nameof(HasRedirectionRules));
            this.OnPropertyChanged(nameof(HasPsf));
        }

        private void TraceRulesOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            this.OnPropertyChanged(nameof(HasTraceRules));
            this.OnPropertyChanged(nameof(HasPsf));
        }

        private void ElectronRulesOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            this.OnPropertyChanged(nameof(HasElectronRules));
            this.OnPropertyChanged(nameof(HasPsf));
        }
    }
}
