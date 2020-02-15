using System.Windows.Media;
using otor.msixhero.lib.Infrastructure;
using otor.msixhero.lib.Infrastructure.Configuration;
using otor.msixhero.lib.Infrastructure.Helpers;
using otor.msixhero.ui.Domain;

namespace otor.msixhero.ui.Modules.Settings.ViewModel.Tools
{
    public class ToolViewModel : ChangeableContainer
    {
        private readonly ToolListConfiguration model;

        public ToolViewModel(IInteractionService interactionService, ToolListConfiguration model)
        {
            this.model = model;
            
            this.AddChildren(
                this.Path = new ChangeableFileProperty(interactionService, model.Path),
                this.Name = new ChangeableProperty<string>(model.Name),
                this.Icon = new ChangeableFileProperty(interactionService, model.Icon)
            );

            this.Path.ValueChanged += (sender, args) => { this.OnPropertyChanged(nameof(Image)); };
        }

        public ChangeableFileProperty Path { get; }

        public ChangeableProperty<string> Name { get; }

        public ChangeableFileProperty Icon { get; }
        public ImageSource Image => ShellIcon.GetIconFor(this.Path.CurrentValue);
    }
}
