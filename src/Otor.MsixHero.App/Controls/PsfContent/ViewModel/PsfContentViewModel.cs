// MSIX Hero
// Copyright (C) 2021 Marcin Otorowski
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// Full notice:
// https://github.com/marcinotorowski/msix-hero/blob/develop/LICENSE.md

using System.Collections.Specialized;
using System.Linq;
using Otor.MsixHero.App.Controls.PsfContent.ViewModel.Items.Custom;
using Otor.MsixHero.App.Controls.PsfContent.ViewModel.Items.Electron;
using Otor.MsixHero.App.Controls.PsfContent.ViewModel.Items.Redirection;
using Otor.MsixHero.App.Controls.PsfContent.ViewModel.Items.Trace;
using Otor.MsixHero.App.Mvvm.Changeable;
using Otor.MsixHero.Appx.Psf.Entities;

namespace Otor.MsixHero.App.Controls.PsfContent.ViewModel
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
