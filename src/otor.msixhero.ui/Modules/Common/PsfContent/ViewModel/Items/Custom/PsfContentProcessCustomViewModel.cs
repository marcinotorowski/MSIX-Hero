using System.Text.RegularExpressions;
using Otor.MsixHero.Appx.Psf.Entities;
using Otor.MsixHero.Ui.Domain;

namespace Otor.MsixHero.Ui.Modules.Common.PsfContent.ViewModel.Items.Custom
{
    public class PsfContentProcessCustomViewModel : PsfContentProcessViewModel
    {
        private readonly ChangeableProperty<string> json;

        public PsfContentProcessCustomViewModel(string processRegularExpression, string fixupName, CustomPsfFixupConfig customFixup) : base(processRegularExpression, fixupName)
        {
            this.json = new ChangeableProperty<string>(customFixup.Json);
            this.AddChild(this.json);
            this.Header = Regex.Replace(fixupName, @"[x_]*(?:64|86|32)?(?:\.dll)?$", string.Empty).ToUpperInvariant();
        }

        public string Header { get; }

        public string Json
        {
            get => this.json.CurrentValue;
            set
            {
                if (this.json.CurrentValue == value)
                {
                    return;
                }

                this.json.CurrentValue = value;
                this.OnPropertyChanged();
            }
        }
    }
}
