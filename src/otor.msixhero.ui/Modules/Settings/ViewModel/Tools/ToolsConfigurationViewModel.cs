using System.Linq;
using otor.msixhero.lib.Infrastructure;
using otor.msixhero.lib.Infrastructure.Configuration;
using otor.msixhero.ui.Domain;

namespace otor.msixhero.ui.Modules.Settings.ViewModel.Tools
{
    public class ToolsConfigurationViewModel : ChangeableContainer
    {
        private readonly IInteractionService interactionService;

        public ToolsConfigurationViewModel(IInteractionService interactionService, Configuration configuration)
        {
            this.interactionService = interactionService;

            var items = configuration?.List?.Tools;
            if (items == null)
            {
                this.Items = new ChangeableCollection<ToolViewModel>();
            }
            else
            {
                this.Items = new ChangeableCollection<ToolViewModel>(items.Select(item => new ToolViewModel(this.interactionService, item)));
            }

            this.AddChild(this.Items);
        }

        public ChangeableCollection<ToolViewModel> Items { get; }
    }
}
