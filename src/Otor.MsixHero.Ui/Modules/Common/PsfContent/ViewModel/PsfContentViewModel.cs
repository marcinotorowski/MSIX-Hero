using System.Collections.Specialized;
using System.Linq;
using Otor.MsixHero.Appx.Psf.Entities;
using Otor.MsixHero.Ui.Domain;
using Otor.MsixHero.Ui.Modules.Common.PsfContent.ViewModel.Items.Custom;
using Otor.MsixHero.Ui.Modules.Common.PsfContent.ViewModel.Items.Electron;
using Otor.MsixHero.Ui.Modules.Common.PsfContent.ViewModel.Items.Redirection;
using Otor.MsixHero.Ui.Modules.Common.PsfContent.ViewModel.Items.Trace;

namespace Otor.MsixHero.Ui.Modules.Common.PsfContent.ViewModel
{
    public class PsfContentViewModel : ChangeableContainer
    {
        public PsfContentViewModel(PsfConfig psfConfig)
        {
            this.RedirectionRules = new ChangeableCollection<PsfContentProcessRedirectionViewModel>();
            this.TraceRules = new ChangeableCollection<PsfContentProcessTraceViewModel>();
            this.ElectronRules = new ChangeableCollection<PsfContentProcessElectronViewModel>();
            this.CustomRules = new ChangeableCollection<PsfContentProcessCustomViewModel>();

            this.Setup(psfConfig);

            this.RedirectionRules.Commit();
            this.TraceRules.Commit();
            this.ElectronRules.Commit();
            this.CustomRules.Commit();
            this.AddChildren(this.RedirectionRules, this.TraceRules, this.ElectronRules, this.CustomRules);

            this.RedirectionRules.CollectionChanged += this.RedirectionRulesOnCollectionChanged;
            this.ElectronRules.CollectionChanged += this.ElectronRulesOnCollectionChanged;
            this.TraceRules.CollectionChanged += this.TraceRulesOnCollectionChanged;
            this.CustomRules.CollectionChanged += this.CustomRulesOnCollectionChanged;
        }

        public ChangeableCollection<PsfContentProcessRedirectionViewModel> RedirectionRules { get; }

        public ChangeableCollection<PsfContentProcessTraceViewModel> TraceRules { get; }

        public ChangeableCollection<PsfContentProcessElectronViewModel> ElectronRules { get; }

        public ChangeableCollection<PsfContentProcessCustomViewModel> CustomRules { get; }

        public bool HasTraceRules => this.TraceRules.Any();

        public bool HasRedirectionRules => this.RedirectionRules.Any();

        public bool HasElectronRules => this.RedirectionRules.Any();

        public bool HasCustomRules => this.CustomRules.Any();

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
                        else if (item.Config is CustomPsfFixupConfig customConfig)
                        {
                            var psfContentProcessViewModel = new PsfContentProcessCustomViewModel(process.Executable, item.Dll, customConfig);
                            this.CustomRules.Add(psfContentProcessViewModel);
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

        private void CustomRulesOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            this.OnPropertyChanged(nameof(HasCustomRules));
            this.OnPropertyChanged(nameof(HasPsf));
        }

        private void ElectronRulesOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            this.OnPropertyChanged(nameof(HasElectronRules));
            this.OnPropertyChanged(nameof(HasPsf));
        }
    }
}
