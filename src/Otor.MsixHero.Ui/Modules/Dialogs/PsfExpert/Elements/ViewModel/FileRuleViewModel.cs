﻿using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Otor.MsixHero.Infrastructure.Progress;
using Otor.MsixHero.Infrastructure.Services;
using Otor.MsixHero.Ui.Controls.ChangeableDialog.ViewModel;
using Otor.MsixHero.Ui.Modules.Common.PsfContent.Model;
using Prism.Services.Dialogs;

namespace Otor.MsixHero.Ui.Modules.Dialogs.PsfExpert.Elements.ViewModel
{
    public class FileRuleViewModel : ChangeableDialogViewModel, IDialogAware
    {
        private InterpretationResult interpretation;
        private string name;
        private string extension;
        private string regularExpression;

        public FileRuleViewModel(IInteractionService interactionService) : base("File pattern rule", interactionService)
        {
        }

        public void OnDialogOpened(IDialogParameters parameters)
        {
            var regexp = parameters.Keys.FirstOrDefault();
            if (regexp == null || !parameters.TryGetValue(regexp, out string value))
            {
                return;
            }

            var interpreter = new RegexpInterpreter(value, true);
            this.Interpretation = interpreter.Result;

            switch (this.interpretation)
            {
                case InterpretationResult.Any:
                    break;
                case InterpretationResult.Extension:
                    this.Extension = interpreter.EditText;
                    this.RegularExpression = interpreter.RegularExpression;
                    break;
                case InterpretationResult.Name:
                    this.Name = interpreter.EditText;
                    this.RegularExpression = interpreter.RegularExpression;
                    break;
                case InterpretationResult.Custom:
                    this.RegularExpression = interpreter.RegularExpression;
                    break;
            }
        }

        public string RegularExpression
        {
            get => this.regularExpression;
            set => this.SetField(ref this.regularExpression, value);
        }

        public string Extension
        {
            get => this.extension;
            set => this.SetField(ref this.extension, value);
        }

        public string Name
        {
            get => this.name;
            set => this.SetField(ref this.name, value);
        }

        public InterpretationResult Interpretation
        {
            get => this.interpretation;
            set => this.SetField(ref this.interpretation, value);
        }

        protected override Task<bool> Save(CancellationToken cancellationToken, IProgress<ProgressData> progress)
        {
            return Task.FromResult(true);
        }
    }
}