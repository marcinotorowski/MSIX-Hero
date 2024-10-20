using System.Collections.Generic;
using System.Linq;
using Otor.MsixHero.Appx.Reader.Psf.Entities.Interpreter.Custom;
using Otor.MsixHero.Appx.Reader.Psf.Entities.Interpreter.Electron;
using Otor.MsixHero.Appx.Reader.Psf.Entities.Interpreter.Redirection;
using Otor.MsixHero.Appx.Reader.Psf.Entities.Interpreter.Trace;

namespace Otor.MsixHero.Appx.Reader.Psf.Entities.Interpreter
{
    public class InterpretedPsf
    {
        private readonly List<PsfRedirectionFixup> _redirectionRules = new();
        private readonly List<PsfTraceFixup> _traceRules = new();
        private readonly List<PsfElectronFixup> _electronRules = new();
        private readonly List<PsfCustomFixup> _customRules = new();

        public InterpretedPsf(PsfConfig psfConfig)
        {
            this.SetFromPsfConfig(psfConfig);
        }

        private void SetFromPsfConfig(PsfConfig psfConfig)
        {
            if (psfConfig?.Processes != null)
            {
                foreach (var process in psfConfig.Processes)
                {
                    foreach (var item in process.Fixups.Where(f => f.Config != null))
                    {
                        if (item.Config is PsfRedirectionFixupConfig redirectionConfig)
                        {
                            var psf = new PsfRedirectionFixup(PsfRedirectionRule.EnumerateRules(redirectionConfig.RedirectedPaths), process.Executable, item.Dll);
                            this._redirectionRules.Add(psf);
                        }
                        else if (item.Config is PsfTraceFixupConfig traceConfig)
                        {
                            var psf = new PsfTraceFixup(traceConfig, process.Executable, item.Dll);
                            this._traceRules.Add(psf);
                        }
                        else if (item.Config is PsfElectronFixupConfig)
                        {
                            var psf = new PsfElectronFixup(process.Executable, item.Dll);
                            this._electronRules.Add(psf);
                        }
                        else if (item.Config is CustomPsfFixupConfig customConfig)
                        {
                            var psf = new PsfCustomFixup(customConfig.Json, process.Executable, item.Dll);
                            this._customRules.Add(psf);
                        }
                    }
                }
            }
        }

        public IReadOnlyList<PsfRedirectionFixup> RedirectionRules => this._redirectionRules;
        public IReadOnlyList<PsfTraceFixup> TraceRules => this._traceRules;
        public IReadOnlyList<PsfElectronFixup> ElectronRules => this._electronRules;
        public IReadOnlyList<PsfCustomFixup> CustomRules => this._customRules;
    }
}